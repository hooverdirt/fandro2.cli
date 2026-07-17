# FandroLib

A high-performance file search library using Boyer-Moore-Horspool string matching with memory-mapped file I/O. Officially now ported so that it can run on Windows/Mac/Linux. This is my ongoing study project apparently.

## Future plans

I was always planning to remove the Windows specific code (see also Fandro2's readme: https://github.com/hooverdirt/fandro2 ) and this repo is the first official attempt to do so. Maybe in the near future we'll incorporate this into Terminal.Gui or move this to Rust.

As usually, BMH code is hand coded and has survived way too many tests.


# Official stuff follows

## Project Structure

```
FandroLib/
├── FandroSearch/          # Core search library
│   ├── Finders/
│   │   ├── Finding/       # FileFinder, FolderProcessingEventArgs
│   │   ├── Matching/      # StringMatcher, FileSizeMatcher, DateTimeMatcher, FileFilters
│   │   └── Threading/     # FoldersWordFinder, FileSetWordFinder, SearchReport
│   └── ...
├── FandroSearch.Tests/    # Unit and integration tests
├── Fandro.Cli/            # Command-line executable
└── FandroLib.slnx         # Solution file
```

## CLI Usage

The `fandro-cli` tool provides a command-line interface to the search library.

### Arguments

| Flag | Long Form | Required | Description | Default |
|---|---|---|---|---|
| `-f` | `--folder` | **Yes** | Starting folder to search | (none — must be provided) |
| `-R` | `--recurse` | No | Enable recursive subdirectory search | `false` |
| `-m` | `--mask` | No | File mask pattern | `*.*` |
| (positional) | — | No | Search term to find in file contents | (empty — lists matching files) |

### Examples

```bash
# Search for "TODO" in all .txt files under /projects, recursively
fandro-cli -f /projects -R -m "*.txt" "TODO"

# List all .log files under /logs (no content search)
fandro-cli -f /logs -m "*.log"

# Search "ERROR" in all files under current directory
fandro-cli -f . "ERROR"

# Search with custom mask
fandro-cli -f /var/log -R -m "*.log" "connection refused"
```

### Output

- Matching file paths are printed to **stdout**, one per line
- Progress and error messages go to **stderr**
- If no search term is provided, all files matching the mask are listed

## Building

```bash
dotnet build
dotnet test
```

## Publishing

### macOS (Intel — x86_64)

```bash
# Standalone executable (requires .NET runtime installed)
dotnet publish Fandro.Cli -c Release -r osx-x64 -o ./publish
./publish/fandro-cli -f /path -m "*.txt" "search term"
```

### macOS (Apple Silicon — arm64)

```bash
# Standalone executable (requires .NET runtime installed)
dotnet publish Fandro.Cli -c Release -r osx-arm64 -o ./publish
./publish/fandro-cli -f /path -m "*.txt" "search term"
```

### Windows

```bash
dotnet publish Fandro.Cli -c Release -r win-x64 -o ./publish
```

## Running the Published App

### macOS (Intel — x86_64)

```bash
./publish/fandro-cli -f /path -m "*.txt" "search term"
```

### macOS (Apple Silicon — arm64)

```bash
./publish/fandro-cli -f /path -m "*.txt" "search term"
```

### Windows

```cmd
publish\fandro-cli.exe -f C:\path -m "*.txt" "search term"
```

> **Note:** The published executable is a native binary that requires the .NET runtime to be installed on the target machine. It runs directly — no `dotnet` command needed.

## Library Usage

```csharp
using FandroSearch.Finders.Matching;
using FandroSearch.Finders.Threading;

FoldersWordFinder finder = new FoldersWordFinder {
    StartFolder = "/path/to/search",
    Mask = "*.txt",
    Recursive = true,
    Pattern = "TODO",
    Conditions = new FileFilters { /* optional filters */ }
};

List<SearchReport> results = new List<SearchReport>();
Progress<SearchReport> progress = new Progress<SearchReport>(results.Add);

await finder.DoWorkAsync(progress, CancellationToken.None);
```
