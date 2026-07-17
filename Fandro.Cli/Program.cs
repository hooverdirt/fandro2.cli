using FandroSearch.Finders.Matching;
using FandroSearch.Finders.Threading;

System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Error));

// ─── Argument Parsing ───────────────────────────────────────────────
string? folder = null;
bool recursive = true;
string mask = "*.*";
string? pattern = null;

string[] commandArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
int i = 0;
while (i < commandArgs.Length)
{
    string arg = commandArgs[i];

    if (arg is "-f" or "--folder")
    {
        if (i + 1 >= commandArgs.Length)
        {
            Console.Error.WriteLine("Error: -f/--folder requires a value.");
            return 1;
        }
        folder = commandArgs[++i];
    }
    else if (arg is "-R" or "--recurse")
    {
        recursive = true;
    }
    else if (arg is "-m" or "--mask")
    {
        if (i + 1 >= commandArgs.Length)
        {
            Console.Error.WriteLine("Error: -m/--mask requires a value.");
            return 1;
        }
        mask = commandArgs[++i];
    }
    else if (arg.StartsWith("-"))
    {
        Console.Error.WriteLine($"Error: Unknown option '{arg}'.");
        PrintUsage();
        return 1;
    }
    else
    {
        // Positional argument = search pattern
        pattern = arg;
    }

    i++;
}

// Validate required arguments
if (string.IsNullOrEmpty(folder))
{
    Console.Error.WriteLine("Error: Folder path is required. Use -f or --folder.");
    PrintUsage();
    return 1;
}

// ─── Search ─────────────────────────────────────────────────────────
FoldersWordFinder finder = new FoldersWordFinder
{
    StartFolder = folder!,
    Mask = mask,
    Recursive = recursive,
    Pattern = pattern ?? ""
};

List<string> fileMatches = new List<string>();

Progress<SearchReport> progress = new Progress<SearchReport>(report =>
{
    switch (report)
    {
        case FileMatchReport fileMatch:
            fileMatches.Add(fileMatch.FileInfo.FullName);
            break;
        case StatusReport status:
            // Console.Error.Write($"\r{status.Text}...");
            break;
        case ErrorReport error:
            Console.Error.WriteLine("Error: " + error.Message);
            break;
    }
});

await finder.DoWorkAsync(progress, CancellationToken.None);


// ─── Output ─────────────────────────────────────────────────────────
foreach (string path in fileMatches)
{
    Console.WriteLine(path);
}

return 0;

static void PrintUsage()
{
    Console.Error.WriteLine("""
Usage: fandro-cli -f <folder> [-R] [-m <mask>] [<search term>]

Required:
  -f, --folder    Starting folder to search

Options:
  -R, --recurse   Enable recursive subdirectory search
  -m, --mask      File mask pattern (default: *.* )

Positional:
  <search term>   Text pattern to find in file contents (optional)
                   If omitted, lists all files matching the mask.
""");
}
