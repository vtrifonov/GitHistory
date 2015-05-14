
using System.Text.RegularExpressions;
namespace GitHistory.Parsing
{
    public class Author
    {
        public static Regex AutorRegex = new Regex("(?<Name>[^<]*)\\s+<(?<Email>[^>]*)>", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public Author(string authorString)
        {
            var match = AutorRegex.Match(authorString);
            if (match.Success)
            {
                this.Name = match.Groups["Name"].Value;
                this.Email = match.Groups["Email"].Value;
            }
            else
            {
                this.Name = authorString;
            }
        }

        public string Name { get; set; }
        public string Email { get; set; }
    }
}
