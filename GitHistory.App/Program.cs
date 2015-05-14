using GitHistory.App.Arguments;
using GitHistory.Parsing;
using RazorTemplates.Core;
using System;
using System.Linq;
using System.IO;

namespace GitHistory.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentsParser();

            var parameters = parser.Parse(args);
            if (parameters == null || !parameters.Validate())
            {
                Environment.Exit(1);
            }

            var commits = new HistoryParser().GetCommits(parameters);

            if (!parameters.IncludeMerges)
            {
                commits = commits.Where(commit => !commit.Headers.Any(header => header.Key == "Merge")).ToList();
            }

            var templateContent = File.ReadAllText(parameters.RazorTemplateFile);
            ITemplate<dynamic> template = null;
            try
            {
                template = Template.Compile(templateContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error compiling template located on {0}. Error message: {1}", parameters.RazorTemplateFile, ex.Message));
                Environment.Exit(2);
            }

            string renderedContent = null;
            try
            {
                renderedContent = template.Render(new { PageTitle = parameters.PageTitle, StartRevision = parameters.StartRevision, EndRevision = parameters.EndRevision, Commits = commits });
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error rendering template. Error message: {0}", ex.Message));
                Environment.Exit(3);
            }

            try
            {
                string outputFolder = Path.GetDirectoryName(parameters.OutputFile);

                if (!string.IsNullOrWhiteSpace(outputFolder) && !Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }
                File.WriteAllText(parameters.OutputFile, renderedContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error saving output file {0}. Error message: {1}", parameters.OutputFile, ex.Message));
                Environment.Exit(4);
            }
        }
    }
}
