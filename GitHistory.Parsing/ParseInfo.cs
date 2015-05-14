
namespace GitHistory.Parsing
{
    public class ParseInfo : IParseInfo
    {
        public string GitRepo { get; set; }
        public string StartRevision { get; set; }
        public string EndRevision { get; set; }
        public string PathInRepo { get; set; }
        public string GitInstallationFolder { get; set; }
    }
}
