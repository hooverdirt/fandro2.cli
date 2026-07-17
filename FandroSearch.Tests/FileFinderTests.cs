using FandroSearch.Finders.Finding;

namespace FandroSearch.Tests;

public class FileFinderTests : IDisposable {
    private readonly string _testDirectory;

    public FileFinderTests() {
        this._testDirectory = Path.Combine(Path.GetTempPath(), "FandroSearch.Tests.FileFinder", Guid.NewGuid().ToString());
        Directory.CreateDirectory(this._testDirectory);
    }

    public void Dispose() {
        if (Directory.Exists(this._testDirectory)) {
            Directory.Delete(this._testDirectory, true);
        }
    }

    private string CreateFile(string relativePath, string content) {
        string path = Path.Combine(this._testDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    #region Basic File Discovery

    [Fact]
    public void Execute_FindsFilesInRootFolder() {
        // Arrange
        CreateFile("a.txt", "content");
        CreateFile("b.cs", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.*",
            Recursive = false
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Equal(2, foundFiles.Count);
    }

    [Fact]
    public void Execute_FilesOnly_MatchesCorrectMask() {
        // Arrange
        CreateFile("a.txt", "content");
        CreateFile("b.cs", "content");
        CreateFile("c.log", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.txt",
            Recursive = false
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Single(foundFiles);
        Assert.Equal("a.txt", foundFiles[0].Name);
    }

    [Fact]
    public void Execute_Recursive_FindsAllFiles() {
        // Arrange
        CreateFile("root.txt", "content");
        CreateFile("sub1/deep.txt", "content");
        CreateFile("sub1/deeper/deepdeeper.txt", "content");
        CreateFile("sub2/another.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.txt",
            Recursive = true
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Equal(4, foundFiles.Count);
    }

    [Fact]
    public void Execute_NonRecursive_SkipsSubfolders() {
        // Arrange
        CreateFile("root.txt", "content");
        CreateFile("sub1/deep.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.txt",
            Recursive = false
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Single(foundFiles);
        Assert.Equal("root.txt", foundFiles[0].Name);
    }

    #endregion

    #region Folder Processing Events

    [Fact]
    public void Execute_FiresFolderProcessingEvent_ForEachFolder() {
        // Arrange
        CreateFile("a.txt", "content");
        CreateFile("sub1/b.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.*",
            Recursive = true
        };

        List<string> processedFolders = new List<string>();
        finder.FolderProcessingEvent += (s, e) => {
            processedFolders.Add(e.DirectoryInfo!.Name);
        };

        // Act
        finder.Execute();

        // Assert
        Assert.True(processedFolders.Count >= 2, $"Expected at least 2 folders, got {processedFolders.Count}");
    }

    [Fact]
    public void Execute_NonRecursive_FiresFolderProcessingEvent_OnlyRoot() {
        // Arrange
        CreateFile("a.txt", "content");
        CreateFile("sub1/b.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.*",
            Recursive = false
        };

        List<string> processedFolders = new List<string>();
        finder.FolderProcessingEvent += (s, e) => {
            processedFolders.Add(e.DirectoryInfo!.Name);
        };

        // Act
        finder.Execute();

        // Assert
        Assert.Single(processedFolders);
    }

    #endregion

    #region Cancellation

    [Fact]
    public void Execute_FileFoundEvent_CancellationStopsSearch() {
        // Arrange
        for (int i = 0; i < 20; i++) {
            CreateFile($"file{i}.txt", "content");
        }
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.txt",
            Recursive = false
        };

        int foundCount = 0;
        finder.FileFoundEvent += (s, e) => {
            foundCount++;
            if (foundCount >= 5) {
                e.Cancelled = true;
            }
        };

        // Act
        finder.Execute();

        // Assert
        Assert.True(foundCount <= 5, $"Should have stopped at 5, found {foundCount}");
    }

    [Fact]
    public void Execute_FolderProcessingEvent_CancellationStopsSearch() {
        // Arrange
        CreateFile("root.txt", "content");
        CreateFile("sub1/a.txt", "content");
        CreateFile("sub2/b.txt", "content");
        CreateFile("sub3/c.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.*",
            Recursive = true
        };

        int folderCount = 0;
        finder.FolderProcessingEvent += (s, e) => {
            folderCount++;
            if (folderCount >= 2) {
                e.Cancelled = true;
            }
        };

        // Act
        finder.Execute();

        // Assert
        Assert.True(folderCount <= 2, $"Should have stopped at 2 folders, processed {folderCount}");
    }

    #endregion

    #region Multiple Folders

    [Fact]
    public void Execute_MultipleFolders_SemicolonSeparated() {
        // Arrange
        string dir1 = Path.Combine(this._testDirectory, "multi_a");
        string dir2 = Path.Combine(this._testDirectory, "multi_b");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        File.WriteAllText(Path.Combine(dir1, "a.txt"), "content");
        File.WriteAllText(Path.Combine(dir2, "b.txt"), "content");

        FileFinder finder = new FileFinder {
            TargetFolder = dir1 + ";" + dir2,
            FileMask = "*.txt",
            Recursive = false
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Equal(2, foundFiles.Count);
    }

    [Fact]
    public void Execute_MultipleFolders_OneNonExistent_SkipsQuietly() {
        // Arrange
        string dir1 = Path.Combine(this._testDirectory, "exists");
        Directory.CreateDirectory(dir1);
        File.WriteAllText(Path.Combine(dir1, "a.txt"), "content");

        FileFinder finder = new FileFinder {
            TargetFolder = dir1 + ";" + Path.Combine(this._testDirectory, "does_not_exist_xyz"),
            FileMask = "*.txt",
            Recursive = false
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act & Assert — should not throw
        Exception ex = Record.Exception(() => finder.Execute());
        Assert.Null(ex);
        Assert.Single(foundFiles);
    }

    #endregion

    #region OnFileFound / OnFolderProcessing

    [Fact]
    public void OnFileFound_InokesHandler() {
        // Arrange
        FileFinder finder = new FileFinder();
        bool called = false;
        finder.FileFoundEvent += (s, e) => called = true;

        // Act
        finder.OnFileFound(new FileFoundEventArgs(new FileInfo(Path.GetTempFileName())));

        // Assert
        Assert.True(called);
    }

    [Fact]
    public void OnFolderProcessing_InokesHandler() {
        // Arrange
        FileFinder finder = new FileFinder();
        bool called = false;
        finder.FolderProcessingEvent += (s, e) => called = true;

        // Act
        finder.OnFolderProcessing(new FolderProcessingEventArgs(new DirectoryInfo(Path.GetTempPath())));

        // Assert
        Assert.True(called);
    }

    [Fact]
    public void OnFileFound_WithNoHandler_DoesNotThrow() {
        // Arrange
        FileFinder finder = new FileFinder();

        // Act & Assert
        Exception ex = Record.Exception(() => finder.OnFileFound(new FileFoundEventArgs(new FileInfo(Path.GetTempFileName()))));
        Assert.Null(ex);
    }

    #endregion

    #region Defaults

    [Fact]
    public void Constructor_DefaultsAreCorrect() {
        // Arrange & Act
        FileFinder finder = new FileFinder();

        // Assert
        Assert.Equal("*", finder.FileMask);
        Assert.Equal("*", finder.FolderMask);
        Assert.False(finder.Recursive);
        Assert.NotNull(finder.FileOptions);
        Assert.NotNull(finder.FolderOptions);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Execute_EmptyTargetFolder_ReturnsNoFiles() {
        // Arrange
        // _testDirectory is created but empty
        FileFinder finder = new FileFinder {
            TargetFolder = _testDirectory,
            FileMask = "*.*",
            Recursive = false
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Empty(foundFiles);
    }

    [Fact]
    public void Execute_NonExistentTargetFolder_DoesNotThrow() {
        // Arrange
        string nonExistent = Path.Combine(_testDirectory, "does_not_exist_xyz");
        FileFinder finder = new FileFinder {
            TargetFolder = nonExistent,
            FileMask = "*.*",
            Recursive = false
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act & Assert — should not throw
        Exception ex = Record.Exception(() => finder.Execute());
        Assert.Null(ex);
        Assert.Empty(foundFiles);
    }

    [Fact]
    public void Execute_FileFoundEvent_CancellationPropagates() {
        // Arrange
        CreateFile("file1.txt", "content");
        CreateFile("file2.txt", "content");
        CreateFile("file3.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = _testDirectory,
            FileMask = "*.txt",
            Recursive = false
        };

        int foundCount = 0;
        bool cancelled = false;
        finder.FileFoundEvent += (s, e) => {
            foundCount++;
            if (foundCount >= 2) {
                e.Cancelled = true;
                cancelled = true;
            }
        };

        // Act
        finder.Execute();

        // Assert
        Assert.True(cancelled, "Cancellation should have been set");
        Assert.True(foundCount >= 2, "Should have found at least 2 files before cancellation");
    }

    [Fact]
    public void Execute_FolderProcessingEvent_CancellationPropagates() {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectory, "sub1"));
        CreateFile("file1.txt", "content");
        CreateFile("sub1/file2.txt", "content");
        CreateFile("sub1/file3.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = _testDirectory,
            FileMask = "*.*",
            Recursive = true
        };

        int folderCount = 0;
        bool cancelled = false;
        finder.FolderProcessingEvent += (s, e) => {
            folderCount++;
            if (folderCount >= 2) {
                e.Cancelled = true;
                cancelled = true;
            }
        };

        // Act
        finder.Execute();

        // Assert
        Assert.True(cancelled, "Cancellation should have been set");
    }

    #endregion

    #region FileOptions / FolderOptions

    [Fact]
    public void Constructor_FileOptions_IsNotNull() {
        // Arrange & Act
        FileFinder finder = new FileFinder();

        // Assert
        Assert.NotNull(finder.FileOptions);
    }

    [Fact]
    public void Constructor_FolderOptions_IsNotNull() {
        // Arrange & Act
        FileFinder finder = new FileFinder();

        // Assert
        Assert.NotNull(finder.FolderOptions);
    }

    [Fact]
    public void SetFileOptions_AppliesCorrectly() {
        // Arrange
        FileFinder finder = new FileFinder();
        EnumerationOptions newOptions = new EnumerationOptions {
            RecurseSubdirectories = true,
            IgnoreInaccessible = false
        };

        // Act
        finder.FileOptions = newOptions;

        // Assert
        Assert.True(finder.FileOptions.RecurseSubdirectories);
        Assert.False(finder.FileOptions.IgnoreInaccessible);
    }

    [Fact]
    public void SetFolderOptions_AppliesCorrectly() {
        // Arrange
        FileFinder finder = new FileFinder();
        EnumerationOptions newOptions = new EnumerationOptions {
            RecurseSubdirectories = true,
            IgnoreInaccessible = false
        };

        // Act
        finder.FolderOptions = newOptions;

        // Assert
        Assert.True(finder.FolderOptions.RecurseSubdirectories);
        Assert.False(finder.FolderOptions.IgnoreInaccessible);
    }

    #endregion

    #region FolderMask

    [Fact]
    public void FolderMask_DefaultIsWildcard() {
        // Arrange & Act
        FileFinder finder = new FileFinder();

        // Assert
        Assert.Equal("*", finder.FolderMask);
    }

    [Fact]
    public void Execute_FolderMaskWildcard_MatchesAllFolders() {
        // Arrange
        CreateFile("dirA/file.txt", "content");
        CreateFile("dirB/file.txt", "content");
        CreateFile("dirC/file.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.txt",
            Recursive = true,
            FolderMask = "*"
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Equal(3, foundFiles.Count);
    }

    [Fact]
    public void Execute_FolderMaskDirA_MatchesOnlyDirA() {
        // Arrange
        CreateFile("dirA/file.txt", "content");
        CreateFile("dirB/file.txt", "content");
        CreateFile("dirC/file.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.txt",
            Recursive = true,
            FolderMask = "dirA*"
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Single(foundFiles);
        Assert.Equal("dirA", foundFiles[0].DirectoryName!.Substring(this._testDirectory.Length + 1).Split(Path.DirectorySeparatorChar)[0]);
    }

    [Fact]
    public void Execute_FolderMaskLogs_MatchesOnlyLogsFolders() {
        // Arrange
        CreateFile("logs/app.log", "content");
        CreateFile("logs/error.log", "content");
        CreateFile("data/record.txt", "content");
        CreateFile("backup/saved.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.*",
            Recursive = true,
            FolderMask = "logs*"
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Equal(2, foundFiles.Count);
    }

    [Fact]
    public void Execute_FolderMaskWildcard_WithRecursive_TraversesAllSubfolders() {
        // Arrange
        CreateFile("root.txt", "content");
        CreateFile("sub1/deep.txt", "content");
        CreateFile("sub1/deeper/deepdeeper.txt", "content");
        CreateFile("sub2/another.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.txt",
            Recursive = true,
            FolderMask = "*"
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert
        Assert.Equal(4, foundFiles.Count);
    }

    [Fact]
    public void Execute_FolderMaskNoMatch_ReturnsNoFiles() {
        // Arrange
        CreateFile("dirA/file.txt", "content");
        CreateFile("dirB/file.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.txt",
            Recursive = true,
            FolderMask = "nomatch*"
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act & Assert — should not throw
        Exception ex = Record.Exception(() => finder.Execute());
        Assert.Null(ex);
        Assert.Empty(foundFiles);
    }

    [Fact]
    public void Execute_FolderMask_AppliedAtEachRecursionLevel() {
        // Arrange
        CreateFile("logs/inner/file.txt", "content");
        CreateFile("logs/logs_backup/deep.log", "content");
        CreateFile("data/file.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.*",
            Recursive = true,
            FolderMask = "logs*"
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert — "inner" doesn't match "logs*" so it's skipped at the second level;
        // only "logs_backup" matches and is traversed
        Assert.Single(foundFiles);
        Assert.Equal("deep.log", foundFiles[0].Name);
    }

    [Fact]
    public void Execute_FolderMask_NonRecursive_IgnoresSubfolders() {
        // Arrange
        CreateFile("file.txt", "content");
        CreateFile("subdir/nested.txt", "content");
        FileFinder finder = new FileFinder {
            TargetFolder = this._testDirectory,
            FileMask = "*.txt",
            Recursive = false,
            FolderMask = "*"
        };

        List<FileInfo> foundFiles = new List<FileInfo>();
        finder.FileFoundEvent += (s, e) => foundFiles.Add(e.FileInfo!);

        // Act
        finder.Execute();

        // Assert — only root-level files; FolderMask has no effect when not recursive
        Assert.Single(foundFiles);
        Assert.Equal("file.txt", foundFiles[0].Name);
    }

    #endregion
}
