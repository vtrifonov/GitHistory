
namespace GitHistory.Parsing
{
    public interface IParseInfo
    {
        string GitRepo { get; set; }
        string StartRevision { get; set; }
        string EndRevision { get; set; }
        string PathInRepo { get; set; }
        string GitInstallationFolder { get; set; }
    }
}
