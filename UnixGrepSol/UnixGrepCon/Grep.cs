using System.Text;
using System.Text.RegularExpressions;

namespace UnixGrepCon
{
    /// <summary>
    /// Helps to search for regex in files.
    /// Similar to the Unix utility "grep".
    /// </summary>
    public class Grep
    {

        /// <summary>Directory to start the search within.</summary>
        private string startDirectory;

        /// <summary>A parameter used eg in "dir" command. The asterisk (*) means all files, asterisk-dot-html means (*.html) means HTML files etc. The default is all files.</summary>
        private string fileFilter;

        /// <summary>True :-: do recurse into subdirectories of the given start directory, false :-: do not dive into subdirs.</summary>
        private bool recurseSubdirectories;

        /// <summary>String to search for in the files (regex).</summary>
        private string searchString;

        /// <summary>Directory names (with possible wildcards) to be excluded from the search.</summary>
        private string[] excludeDirs;

        /// <summary>The regular expression to use during the search operation.</summary>
        private Regex currentRegex;

        /// <summary>A regular expression to tell whether a given directory should be searched through.</summary>
        private Regex excludeDirsRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Grep"/> class.
        /// </summary>
        /// <param name="startDirectory">The start directory.</param>
        /// <param name="fileFilter">The file filter.</param>
        /// <param name="recurseSubdirectories">if set to <c>true</c> [recurse subdirectories].</param>
        /// <param name="searchString">String to search for in the files (regex).</param>
        /// <param name="excludeDirs">Directory names (with possible wildcards) to be excluded from the search.</param>
        public Grep(string startDirectory, string fileFilter, bool recurseSubdirectories, string searchString, string[] excludeDirs)
        {
            this.startDirectory = startDirectory;
            this.fileFilter = fileFilter;
            this.recurseSubdirectories = recurseSubdirectories;
            this.searchString = searchString;
            this.excludeDirs = excludeDirs;
            this.currentRegex = new Regex(this.searchString);
            this.excludeDirsRegex = BuildExcludeDirsRegex(this.excludeDirs);
        }

        /// <summary>
        /// Starts searching the files for occurrences of the given string.
        /// </summary>
        public void StartSearch()
        {
            SearchDirectory(this.startDirectory);
        }

        private void SearchDirectory(string directory)
        {
            // First of all:
            // Check the directory name.
            // If the dir name matches of the "exclude dir names", the directory should be skipped in the search process.
            //Match match = this.excludeDirsRegex.Match(directory);
            Match match = this.excludeDirsRegex.Match(Path.GetFileName(directory));
            bool doSearchThisDirectory = (! match.Success);
            if ( doSearchThisDirectory )
            {
                string[] fileNamesToProcess = Directory.GetFiles(directory, this.fileFilter);
                foreach (string fileName in fileNamesToProcess)
                {
                    SearchInFile(fileName);
                }
                if (this.recurseSubdirectories)
                {
                    string[] directoryNamesToProcess = Directory.GetDirectories(directory);
                    foreach (string directoryName in directoryNamesToProcess)
                    {
                        SearchDirectory(directoryName);
                    }
                }
            }
        }

        private void SearchInFile(string fileName)
        {
            string? fileContents = GetFileContentsAsString(fileName);
            if (fileContents != null)
            {
                //Match match = Regex.Match(fileContents, this.searchString);
                Match match = this.currentRegex.Match(fileContents);
                if (match.Success)
                {
                    // Just write the file name to the console. That's all.
                    Console.WriteLine(fileName);
                }
            }
        }

        // If the result is null, that means the given file IS PROBABLY NOT a text file.
        private string? GetFileContentsAsString(string fileName)
        {
            if (FileHelper.GuessIsTextFile(fileName))
            {
                Encoding encoding = FileHelper.GuessEncoding(fileName);
                string contents = File.ReadAllText(fileName, encoding);
                return contents;
            }
            return null;
        }

        private Regex BuildExcludeDirsRegex(string[] excludeDirs)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('^');
            sb.Append('(');
            if (excludeDirs.Length > 0)
            {
                bool isFirstOption = true;
                foreach (string excludeDir in excludeDirs)
                {
                    // Add an option delimiter first, if necessary.
                    if (isFirstOption)
                    {
                        isFirstOption = false;
                    }
                    else
                    {
                        sb.Append('|');
                    }
                    // Add the dir name converted into a Regex string.
                    string regexSequence = DirNameWithWildcardsToRegexSequence(excludeDir);
                    //sb.Append("(" + regexSequence + ")");
                    sb.Append(regexSequence);
                }
            }
            else
            {
                sb.Append(".*");
            }
            sb.Append(')');
            sb.Append('$');
            string regexPattern = sb.ToString();
            Regex result = new Regex(regexPattern);
            return result;
        }

        private string DirNameWithWildcardsToRegexSequence(string directoryName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in directoryName)
            {
                switch (ch)
                {
                    case '.':
                        sb.Append("\\.");
                        break;

                    case '*':
                        sb.Append(".*");
                        break;

                    case '?':
                        sb.Append(".");
                        break;

                    //case '.':
                    case '^':
                    case '$':
                    //case '*':
                    case '+':
                    //case '?':
                    case '(':
                    case ')':
                    case '[':
                    case '{':
                    case '\\':
                    case '|':
                        sb.Append("\\" + ch.ToString());
                        break;

                    default:
                        sb.Append(ch);
                        break;
                }
            }
            string result = sb.ToString();
            return result;
        }

    }
}
