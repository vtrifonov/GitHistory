using GitHistory.Parsing;
using Ookii.CommandLine;
using System;
using System.IO;
using System.Text;

namespace GitHistory.App.Arguments
{
    public class CommandLineArguments : IParseInfo
    {
        [CommandLineArgument("gitRepo", IsRequired = true, Position = 0), Alias("r")]
        public string GitRepo { get; set; }

        [CommandLineArgument("templateFile", IsRequired = true, Position = 1), Alias("t")]
        public string RazorTemplateFile { get; set; }

        [CommandLineArgument("outputFile", IsRequired = true, Position = 2), Alias("o")]
        public string OutputFile { get; set; }

        [CommandLineArgument("startRevision"), Alias("sr"), Alias("revision")]
        public string StartRevision { get; set; }

        [CommandLineArgument("endRevision"), Alias("er")]
        public string EndRevision { get; set; }

        [CommandLineArgument("includeMerges"), Alias("im")]
        public bool IncludeMerges { get; set; }

        [CommandLineArgument("gitInstallationFolder", DefaultValue=@"C:\Program Files (x86)\Git\bin"), Alias("gi")]
        public string GitInstallationFolder { get; set; }

        [CommandLineArgument("pathInRepo"), Alias("p")]
        public string PathInRepo { get; set; }

        [CommandLineArgument("pageTitle"), Alias("pt")]
        public string PageTitle { get; set; }

        public bool Validate()
        {
            StringBuilder errors = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(this.EndRevision) && string.IsNullOrWhiteSpace(this.StartRevision))
            {
                errors.AppendLine("Please specify startRevision if you are specifying endRevision.");
            }
            if (!Directory.Exists(this.GitRepo))
            {
                errors.AppendLine(string.Format("The given Git repository folder - {0} does not exist!", this.GitRepo));
            }
            if (!Directory.Exists(Path.Combine(this.GitRepo, ".git")))
            {
                errors.AppendLine(string.Format("Git directory(.git) is missing in the repository folder - {0}!", this.GitRepo));
            }
            if (!File.Exists(this.RazorTemplateFile))
            {
                errors.AppendLine(string.Format("The given RazorTemplateFile - {0} does not exist!", this.GitRepo));
            }
            if (!string.IsNullOrWhiteSpace(this.GitInstallationFolder) && !File.Exists(Path.Combine(this.GitInstallationFolder, "git.exe")))
            {
                errors.AppendLine(string.Format("git.exe was not found in the given folder - {0}!", this.GitInstallationFolder));
            }

            var errorsList = errors.ToString();
            if (!string.IsNullOrWhiteSpace(errorsList))
            {
                Console.WriteLine(errorsList);
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
