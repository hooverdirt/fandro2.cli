using FandroSearch.Finders.Matching;
using FandroSearch.Finders.Threading;

namespace FandroSearch.Tests;

public class FoldersWordFinderTests : IDisposable {
    private readonly string _testDirectory;

    public FoldersWordFinderTests() {
        this._testDirectory = Path.Combine(Path.GetTempPath(), "FandroSearch.Tests.Folders", Guid.NewGuid().ToString());
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

    [Fact]
    public async Task DoWorkAsync_FindsMatchInRootFolder() {
        // Arrange
        CreateFile("root.txt", "TODO in root");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = this._testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_FindsMatchInSubfolder_Recursive() {
        // Arrange
        CreateFile("root.txt", "No match here");
        CreateFile("subdir/deep.txt", "TODO in subfolder");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = this._testDirectory,
            Mask = "*.txt",
            Recursive = true,
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_Recursive_False_SkipsSubfolders() {
        // Arrange
        CreateFile("root.txt", "TODO in root");
        CreateFile("subdir/deep.txt", "TODO in subfolder");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = this._testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_MultipleMatchesInSubfolders() {
        // Arrange
        CreateFile("a.txt", "TODO a");
        CreateFile("sub1/b.txt", "TODO b");
        CreateFile("sub1/c.txt", "No match");
        CreateFile("sub2/d.txt", "TODO d");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = this._testDirectory,
            Mask = "*.txt",
            Recursive = true,
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(3, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_ReportsFileMatchReports() {
        // Arrange
        CreateFile("match1.txt", "TODO first");
        CreateFile("match2.txt", "TODO second");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = this._testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        List<FileMatchReport> matchReports = reports.OfType<FileMatchReport>().ToList();
        Assert.Equal(2, matchReports.Count);
    }

    [Fact]
    public async Task DoWorkAsync_NoMatches_ReturnsZero() {
        // Arrange
        CreateFile("clean.txt", "No matches anywhere");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = this._testDirectory,
            Mask = "*.txt",
            Recursive = true,
            Pattern = "XYZNONEXISTENT"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(0, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_StatusReportsAreSent() {
        // Arrange
        CreateFile("a.txt", "Content a");
        CreateFile("b.txt", "Content b");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = this._testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "Content"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        Assert.True(reports.OfType<StatusReport>().Any(), "Should have received status reports");
    }

    #region Conditions/Filter Integration

    [Fact]
    public async Task DoWorkAsync_WithConditions_MatchesSizeFilter() {
        // Arrange
        CreateFile("small.txt", "Hi"); // 2 bytes
        CreateFile("large.txt", "Hello world this is a longer content"); // 36 bytes
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = _testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "Hello"
        };
        FileFilters conditions = new FileFilters();
        conditions.ValidateType = MatcherEnums.MatcherOperator.And;
        conditions.AddMatcher(new FileSizeMatcher {
            CompareValue = 10,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        finder.Conditions = conditions;

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalMatches); // Only large.txt matches
    }

    [Fact]
    public async Task DoWorkAsync_WithConditions_FailsSizeFilter() {
        // Arrange
        CreateFile("small.txt", "Hi"); // 2 bytes
        CreateFile("large.txt", "Hello world this is a longer content"); // 36 bytes
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = _testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "Hello"
        };
        FileFilters conditions = new FileFilters();
        conditions.ValidateType = MatcherEnums.MatcherOperator.And;
        conditions.AddMatcher(new FileSizeMatcher {
            CompareValue = 100,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        finder.Conditions = conditions;

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(0, completed.TotalMatches); // No file is > 100 bytes
    }

    [Fact]
    public async Task DoWorkAsync_WithNullConditions_DoesNotThrow() {
        // Arrange
        CreateFile("test.txt", "Hello world");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = _testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "Hello"
        };
        finder.Conditions = null!;

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act & Assert
        Exception? ex = await Record.ExceptionAsync(async () => await finder.DoWorkAsync(progress, CancellationToken.None));
        Assert.Null(ex);
    }

    #endregion

    #region CaseSensitive Property

    [Fact]
    public async Task DoWorkAsync_CaseSensitive_True_CaseSensitiveSearch() {
        // Arrange
        CreateFile("test.txt", "hello world");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = _testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "HELLO",
            CaseSensitive = true
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(0, completed.TotalMatches); // "HELLO" not found in "hello world" when case-sensitive
    }

    [Fact]
    public async Task DoWorkAsync_CaseSensitive_True_CaseMatches() {
        // Arrange
        CreateFile("test.txt", "HELLO world");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = _testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "HELLO",
            CaseSensitive = true
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_CaseSensitive_False_CaseInsensitiveSearch() {
        // Arrange
        CreateFile("test.txt", "HELLO world");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = _testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = "hello",
            CaseSensitive = false
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_NoPattern_ReportsFilesMatchingMask() {
        // Arrange — condition 2: no pattern set, files should still be reported
        CreateFile("a.txt", "some content");
        CreateFile("b.txt", "other content");
        CreateFile("c.log", "log data");
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = _testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = null! // no search pattern
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(2, completed.TotalMatches); // a.txt and b.txt (not c.log)

        List<FileMatchReport> matchReports = reports.OfType<FileMatchReport>().ToList();
        Assert.Equal(2, matchReports.Count);
        // Position should be -1 since no BMH search was performed
        Assert.All(matchReports, r => Assert.Equal(-1, r.Position));
    }

    [Fact]
    public async Task DoWorkAsync_NoPattern_WithConditions_ReportsOnlyMatchingFiles() {
        // Arrange — condition 2: no pattern + conditions filter
        CreateFile("small.txt", "Hi"); // 2 bytes
        CreateFile("large.txt", "Hello world this is a longer content"); // 36 bytes
        FoldersWordFinder finder = new FoldersWordFinder {
            StartFolder = _testDirectory,
            Mask = "*.txt",
            Recursive = false,
            Pattern = null! // no search pattern
        };
        FileFilters conditions = new FileFilters();
        conditions.ValidateType = MatcherEnums.MatcherOperator.And;
        conditions.AddMatcher(new FileSizeMatcher {
            CompareValue = 10,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        finder.Conditions = conditions;

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalMatches); // Only large.txt passes the size filter

        List<FileMatchReport> matchReports = reports.OfType<FileMatchReport>().ToList();
        Assert.Single(matchReports);
        Assert.Equal(-1, matchReports[0].Position);
    }

    [Fact]
    public void Duration_DefaultIsMinValue() {
        // Arrange & Act
        FoldersWordFinder finder = new FoldersWordFinder();

        // Assert
        Assert.Equal(DateTime.MinValue, finder.Duration);
    }

    [Fact]
    public void Duration_SetAndGet() {
        // Arrange
        FoldersWordFinder finder = new FoldersWordFinder();
        DateTime testTime = new DateTime(2024, 1, 15, 10, 30, 0);

        // Act
        finder.Duration = testTime;

        // Assert
        Assert.Equal(testTime, finder.Duration);
    }

    #endregion
}
