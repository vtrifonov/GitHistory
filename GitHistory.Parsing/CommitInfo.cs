using System.Collections.Generic;

namespace GitHistory.Parsing
{
    public class CommitInfo
    {
        public CommitInfo()
        {
            Headers = new Dictionary<string, string>();
            Files = new List<FileStatus>();
            CommitMessage = string.Empty;
            ConflictFiles = null;
        }

        public Dictionary<string, string> Headers { get; set; }
        public string Sha { get; set; }
        public string CommitMessage { get; set; }
        public List<FileStatus> Files { get; set; }
        public List<string> ConflictFiles { get; set; }

        public Author Author
        {
            get
            {
                if (this.Headers.ContainsKey("Author"))
                {
                    return new Author(this.Headers["Author"]);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
