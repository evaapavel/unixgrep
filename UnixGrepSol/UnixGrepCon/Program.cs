namespace UnixGrepCon
{
    public class Program
    {
        public static void ShowSyntaxAndExit()
        {
            Console.WriteLine("Syntax:");
            Console.WriteLine("UnixGrepCon.exe -s <search-string> [-d <start-directory>] [-f <file-filter>] [-e <exclude-dirs>]");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine("<search-string> means the string to search for within the files.");
            Console.WriteLine("<start-directory> means the directory to start searching in.");
            Console.WriteLine("<file-filter> means rather a file pattern (such as *.cs) that the name of a file must match in order for the file to be included in the search.");
            Console.WriteLine("<exclude-dirs> means a semicolon-delimited list of directories (wildcars are enabled) that should NOT be included in the search.");
            Console.WriteLine();
            Console.WriteLine("Remarks:");
            Console.WriteLine("<search-string>");
            Console.WriteLine("Search string is mandatory.");
            Console.WriteLine("Use back-single-quote for escaped characters such as `t for \\t (TAB).");
            Console.WriteLine("<start-directory>");
            Console.WriteLine("If start dir not specified, the search starts in the current directory.");
            Console.WriteLine("<file-filter>");
            Console.WriteLine("All files are searched by default.");
            Console.WriteLine("<exclude-dirs>");
            Console.WriteLine("Any directory shall be searched for by default.");
            Console.WriteLine("");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("Example #1");
            Console.WriteLine("UnixGrepCon.exe -s \"public static\" -d \"C:\\\" -f \"*.htm*\"");
            Console.WriteLine("Example #2");
            Console.WriteLine("UnixGrepCon.exe -s \"`t`tint x;\" -f \"*\"");
            Console.WriteLine("Example #3");
            Console.WriteLine("UnixGrepCon.exe -s \"string[]\" -d \"C:\\pfoltyn\\PROJECTS\\Gemalto_SSH\\gwes\\deviceagents\\device-agent\\FaceCamera\" -e \"bin;obj;.*\"");
            Console.WriteLine();
            Environment.Exit(1);
        }

        public static void Main(string[] args)
        {
            // There must be at least one parameter, ie two command line args (-s "search-string").
            if ( (args.Length < 2) || (args.Length % 2 == 1) )
            {
                ShowSyntaxAndExit();
            }

            // Parse the parameters.

            string? searchString = null;
            string startDirectory = Directory.GetCurrentDirectory();
            string fileFilter = "*";
            string[] excludeDirs = new string[0];

            bool searchStringSet = false;
            bool startDirectorySet = false;
            bool fileFilterSet = false;
            bool excludeDirsSet = false;

            // Process all command line arguments.
            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-s":
                        if (searchStringSet)
                        {
                            ShowSyntaxAndExit();
                        }
                        searchString = args[i + 1];
                        searchString = searchString.Replace("`", "\\");
                        searchStringSet = true;
                        break;

                    case "-d":
                        if (startDirectorySet)
                        {
                            ShowSyntaxAndExit();
                        }
                        startDirectory = args[i + 1];
                        startDirectorySet = true;
                        break;

                    case "-f":
                        if (fileFilterSet)
                        {
                            ShowSyntaxAndExit();
                        }
                        fileFilter = args[i + 1];
                        fileFilterSet = true;
                        break;

                    case "-e":
                        if (excludeDirsSet)
                        {
                            ShowSyntaxAndExit();
                        }
                        string excludeDirsList = args[i + 1];
                        excludeDirs = excludeDirsList.Split(';');
                        excludeDirsSet = true;
                        break;

                    default:
                        ShowSyntaxAndExit();
                        break;
                }
            }

            // Integrity check.
            if ( ! searchStringSet )
            {
                ShowSyntaxAndExit();
            }

            // Adjust the start directory.
            startDirectory = Path.GetFullPath(startDirectory);

            // Display params:
            //Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Search string:     {searchString}");
            Console.WriteLine($"Start directory:   {startDirectory}");
            Console.WriteLine($"File filter:       {fileFilter}");
            Console.WriteLine($"Exclude dirs:      {string.Join(';', excludeDirs)}");
            Console.WriteLine();

            // Prepare the "grep" search.
            bool recurseSubdirectories = true;
            Grep grep = new Grep(startDirectory, fileFilter, recurseSubdirectories, searchString!, excludeDirs);
            grep.StartSearch();
        }
    }
}