using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GitHistory.Parsing
{
    public class HistoryParser
    {
        public List<CommitInfo> GetCommits(IParseInfo parseInfo)
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

            return GetCommits(list);
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

        private List<CommitInfo> GetCommits(string input)
        {
            CommitInfo commit = null;
            var commits = new List<CommitInfo>();
            bool processingMessage = false;
            using (var strReader = new StringReader(input))
            {
                do
                {
                    var line = strReader.ReadLine();

                    if (line.StartsWith("commit "))
                    {
                        if (commit != null)
                            commits.Add(commit);
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

                    if (string.IsNullOrEmpty(line))
                    {
                        // commit message divider
                        processingMessage = !processingMessage;
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
                commits.Add(commit);
            }

            return commits;
        }
    }
}
