using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitHistory.Parsing
{
    public class HistoryParser
    {
        public static Regex RegexParse = new Regex(
            "commit\\s+(?<Sha>[a-z0-9A-Z]*)\\s*(?:Merge:\\s+(?<Merge>[a-z" +
            "A-Z0-9]*\\s+[a-zA-Z0-9]*)\\s*)?Author:\\s+(?<AuthorName>[^<]*" +
            ")\\s+<(?<AuthorEmail>[^>]*)>\\s*Date:\\s*(?<Date>[^\\n]*)\\s*" +
            "(?<CommitMessage>(?:(?!^[AMD]\\t)(?!^\\s+Conflicts)(?!^commi" +
            "t).*$\\n)+)\\s*(?:Conflicts:\\s*(?<Conflicts>(?!^[AMD]\\t)[^" +
            "\\n]*\\.[\\w]*\\s*)+)?\\s*(?:(?<FilesInfo>[MDA]\\s*[^\\n]*)\\n" +
            ")*",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static Regex RegexFileInfo = new Regex("(?<Status>[AMD])\\s+(?<FileInfo>.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static Regex RegexCommits = new Regex("(?<! )commit\\s+(?<Sha>[a-z0-9A-Z]*)\\s*", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public IEnumerable<CommitInfo> GetCommits(IParseInfo parseInfo)
        {
            string revision = string.Empty;

            if (!string.IsNullOrWhiteSpace(parseInfo.StartRevision))
            {
                revision = " " + parseInfo.StartRevision;
                if (!string.IsNullOrWhiteSpace(parseInfo.EndRevision))
                {
                    revision += ".." + parseInfo.EndRevision;
                }
            }

            string path = string.Empty;
            if (!string.IsNullOrWhiteSpace(parseInfo.PathInRepo))
            {
                path = " -- " + parseInfo.PathInRepo.Replace("\\", "/");
            }

            string arguments = string.Format(" --git-dir={0}/.git --work-tree={0} log --name-status{1}{2}", parseInfo.GitRepo.Replace("\\", "/"), revision, path);

            var list = ListShaWithFiles(parseInfo.GitInstallationFolder, arguments);

            //return GetCommits(list);

            return GetCommitsRegex(list);
        }

        private string ListShaWithFiles(string gitInstallationFolder, string arguments)
        {
            var output = RunProcess(gitInstallationFolder, arguments);
            return output;
        }

        private bool StartsWithHeader(string line)
        {
            if (line.Length > 0 && char.IsLetter(line[0]))
            {
                var seq = line.SkipWhile(ch => Char.IsLetter(ch) && ch != ':');
                return seq.FirstOrDefault() == ':';
            }
            return false;
        }

        private string RunProcess(string gitInstallationFolder, string command)
        {
            // Start the child process.
            Process process = new Process();
            // Redirect the output stream of the child process.
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = command;
            process.StartInfo.WorkingDirectory = gitInstallationFolder;
            process.Start();
            // Read the output stream first and then wait.
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        private IEnumerable<CommitInfo> GetCommits(string input)
        {
            CommitInfo commit = null;
            using (var strReader = new StringReader(input))
            {
                do
                {
                    var line = strReader.ReadLine();

                    if (line.StartsWith("commit "))
                    {
                        if (commit != null)
                        {
                            yield return commit;
                        }
                            
                        commit = new CommitInfo();
                        commit.Sha = line.Split(' ')[1];
                    }

                    if (StartsWithHeader(line))
                    {
                        var header = line.Split(':')[0];
                        var val = string.Join(":", line.Split(':').Skip(1)).Trim();

                        // headers
                        commit.Headers.Add(header, val);
                    }

                    if (line.Length > 0 && line.Trim().Length > 0 && (line[0] == '\t' || line[0] == ' '))
                    {
                        var lineText = line.Trim();

                        if (commit.ConflictFiles != null)
                        {
                            // conflicts message.
                            commit.ConflictFiles.Add(lineText);
                        }
                        else
                        {
                            if (lineText.StartsWith("Conflicts:"))
                            {
                                // conflicts message.
                                commit.ConflictFiles = new List<string>();
                            }
                            else
                            {
                                // commit message.
                                if (string.IsNullOrWhiteSpace(commit.CommitMessage))
                                {
                                    commit.CommitMessage = line.Trim();
                                }
                                else
                                {
                                    commit.CommitMessage += " " + line.Trim();
                                }
                            }
                        }
                    }

                    if (line.Length > 1 && Char.IsLetter(line[0]) && line[1] == '\t')
                    {
                        var status = line.Split('\t')[0];
                        var file = line.Split('\t')[1];
                        commit.Files.Add(new FileStatus() { Status = status, File = file });
                    }
                }
                while (strReader.Peek() != -1);
            }
            if (commit != null)
            {
                if (commit.ConflictFiles == null)
                {
                    commit.ConflictFiles = new List<string>();
                }
                yield return commit;
            }
        }

        private IEnumerable<CommitInfo> GetCommitsRegex(string input)
        {
            var matches = RegexParse.Matches(input);

            var commitMatches = RegexCommits.Matches(input);

            // check whether the parsing regex has parsed all the commits
            if (matches.Count != commitMatches.Count)
            {
                var matchedSHAs = matches.Cast<Match>().Select(x => x.Groups["Sha"].Value);
                var allSHAs = commitMatches.Cast<Match>().Select(x => x.Groups["Sha"].Value);

                var notMatched = allSHAs.Except(matchedSHAs);

                throw new Exception(string.Format("Not all commits are matched! Please, review the parsing regex! The SHAs of the not matched commits are: {0}", 
                    string.Join(", ", notMatched)));
            }

            foreach (Match match in matches)
            {
                var commit = new CommitInfo
                {
                    Author = new Author
                    {
                        Name = match.Groups["AuthorName"].Value.Trim(),
                        Email = match.Groups["AuthorEmail"].Value.Trim()
                    },
                    CommitMessage = System.Text.RegularExpressions.Regex.Replace(match.Groups["CommitMessage"].Value.Trim(), @"\s+"," "),
                    Sha = match.Groups["Sha"].Value.Trim(),
                    ConflictFiles = new List<string>()
                };

                var merge = match.Groups["Merge"].Value.Trim();
                var date = match.Groups["Date"].Value.Trim();
                
                foreach (Capture conflict in match.Groups["Conflicts"].Captures)
                {
                    commit.ConflictFiles.Add(conflict.Value);
	            }

                foreach (Capture fileInfo in match.Groups["FilesInfo"].Captures)
                {
                    var fileInfoMatch = RegexFileInfo.Match(fileInfo.Value);
                    if (!fileInfoMatch.Success)
                    {
                        throw new Exception(string.Format("Cannot find FileInfo from string: {0}", fileInfo.Value));
                    }
                    commit.Files.Add(new FileStatus 
                    {
                        File = fileInfoMatch.Groups["FileInfo"].Value,
                        Status = fileInfoMatch.Groups["Status"].Value
                    });
                }

                if (!string.IsNullOrWhiteSpace(merge))
                {
                    commit.Headers.Add("Merge", merge);
                }

                if (!string.IsNullOrWhiteSpace(date))
                {
                    commit.Headers.Add("Date", merge);
                }

                yield return commit;
            }
        }
    }
}
