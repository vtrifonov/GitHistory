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
            var template = Template.Compile(templateContent);
            var renderedContent = template.Render(new { PageTitle = parameters.PageTitle, StartRevision = parameters.StartRevision, EndRevision = parameters.EndRevision, Commits = commits });

            string outputFolder = Path.GetDirectoryName(parameters.OutputFile);

            if (!string.IsNullOrWhiteSpace(outputFolder) && !Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            File.WriteAllText(parameters.OutputFile, renderedContent);
        }
    }
}
