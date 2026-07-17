using FandroSearch.Finders.Matching;
using FandroSearch.Finders.Threading;

namespace FandroSearch.Tests;

public class FileSetWordFinderTests : IDisposable {
    private readonly string _testDirectory;

    public FileSetWordFinderTests() {
        this._testDirectory = Path.Combine(Path.GetTempPath(), "FandroSearch.Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(this._testDirectory);
    }

    public void Dispose() {
        if (Directory.Exists(this._testDirectory)) {
            Directory.Delete(this._testDirectory, true);
        }
    }

    private string CreateFile(string name, string content) {
        string path = Path.Combine(this._testDirectory, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public async Task DoWorkAsync_FindsSingleMatch() {
        // Arrange
        string file1 = CreateFile("test1.txt", "Hello world TODO marker");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalFilesScanned);
        Assert.Equal(1, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_FindsMatchesAcrossMultipleFiles() {
        // Arrange
        string file1 = CreateFile("test1.txt", "TODO in file one");
        string file2 = CreateFile("test2.txt", "No match here");
        string file3 = CreateFile("test3.txt", "TODO in file three");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1, file2, file3],
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(3, completed.TotalFilesScanned);
        Assert.Equal(2, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_NoMatch_ReturnsZeroMatches() {
        // Arrange
        string file1 = CreateFile("test1.txt", "Hello world no matches here");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = "XYZNONEXISTENT"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalFilesScanned);
        Assert.Equal(0, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_EmptyFile_IsSkipped() {
        // Arrange
        string file1 = CreateFile("empty.txt", "");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalFilesScanned);
        Assert.Equal(0, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_Cancellation_StopsBeforeCompletion() {
        // Arrange
        List<string> files = Enumerable.Range(0, 100)
            .Select(i => CreateFile($"file{i}.txt", $"Content {i}"))
            .ToList();

        CancellationTokenSource cts = new CancellationTokenSource();
        // Cancel immediately
        cts.Cancel();
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = files,
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act & Assert - should throw immediately
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            finder.DoWorkAsync(progress, cts.Token));
    }

    [Fact]
    public async Task DoWorkAsync_CaseInsensitiveByDefault() {
        // Arrange
        string file1 = CreateFile("test1.txt", "Hello WORLD marker");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = "world",
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
    public async Task DoWorkAsync_CaseSensitive_DoesNotMatch() {
        // Arrange
        string file1 = CreateFile("test1.txt", "Hello WORLD marker");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = "world",
            CaseSensitive = true
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
    public async Task DoWorkAsync_ReportsFileMatchReport() {
        // Arrange
        string file1 = CreateFile("test1.txt", "Hello TODO world");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        Assert.Single(reports.OfType<FileMatchReport>());
        FileMatchReport match = reports.OfType<FileMatchReport>().First();
        Assert.Equal(file1, match.FileInfo.FullName);
        Assert.True(match.Position >= 0);
    }

    [Fact]
    public async Task DoWorkAsync_NonExistentFile_IsSkipped() {
        // Arrange
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [Path.Combine(this._testDirectory, "does_not_exist.txt")],
            Pattern = "TODO"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalFilesScanned);
        Assert.Equal(0, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_ReportsStatusAndCompleted() {
        // Arrange
        string file1 = CreateFile("test1.txt", "Content");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = "Content"
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        Assert.True(reports.Any(r => r is StatusReport), "Should have status report");
        Assert.True(reports.Any(r => r is SearchCompletedReport), "Should have completion report");
        Assert.True(reports.Any(r => r is FileMatchReport), "Should have match report");
    }

    #region Conditions/Filter Integration

    [Fact]
    public async Task DoWorkAsync_WithConditions_MatchesSizeFilter() {
        // Arrange
        string file1 = CreateFile("small.txt", "Hi"); // 2 bytes
        string file2 = CreateFile("large.txt", "Hello world this is a longer content"); // 36 bytes
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1, file2],
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
        string file1 = CreateFile("small.txt", "Hi"); // 2 bytes
        string file2 = CreateFile("large.txt", "Hello world this is a longer content"); // 36 bytes
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1, file2],
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
    public async Task DoWorkAsync_WithConditions_MultipleMatchers_And() {
        // Arrange
        string file1 = CreateFile("test.txt", "Hello TODO world"); // 16 bytes
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = "TODO"
        };
        FileFilters conditions = new FileFilters();
        conditions.ValidateType = MatcherEnums.MatcherOperator.And;
        conditions.AddMatcher(new FileSizeMatcher {
            CompareValue = 10,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        conditions.AddMatcher(new FileSizeMatcher {
            CompareValue = 20,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Less,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        finder.Conditions = conditions;

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_WithConditions_OrFilter() {
        // Arrange
        string file1 = CreateFile("test.txt", "Hi TODO"); // 7 bytes
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = "TODO"
        };
        FileFilters conditions = new FileFilters();
        conditions.ValidateType = MatcherEnums.MatcherOperator.Or;
        conditions.AddMatcher(new FileSizeMatcher {
            CompareValue = 1000,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        conditions.AddMatcher(new FileSizeMatcher {
            CompareValue = 10,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Less,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        finder.Conditions = conditions;

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(1, completed.TotalMatches);
    }

    [Fact]
    public async Task DoWorkAsync_WithNullConditions_DoesNotThrow() {
        // Arrange
        string file1 = CreateFile("test.txt", "Hello world");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
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

    #region Empty Pattern

    [Fact]
    public async Task DoWorkAsync_EmptyPattern_ReturnsNoMatches() {
        // Arrange
        string file1 = CreateFile("test.txt", "Hello world TODO marker");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = ""
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
    public async Task DoWorkAsync_NullPattern_ReturnsNoMatches() {
        // Arrange
        string file1 = CreateFile("test.txt", "Hello world TODO marker");
        FileSetWordFinder finder = new FileSetWordFinder {
            Fileset = [file1],
            Pattern = null!
        };

        List<SearchReport> reports = new List<SearchReport>();
        Progress<SearchReport> progress = new Progress<SearchReport>(reports.Add);

        // Act
        await finder.DoWorkAsync(progress, CancellationToken.None);

        // Assert
        SearchCompletedReport completed = Assert.Single(reports.OfType<SearchCompletedReport>());
        Assert.Equal(0, completed.TotalMatches);
    }

    #endregion

    #region Duration Property

    [Fact]
    public void Duration_DefaultIsMinValue() {
        // Arrange & Act
        FileSetWordFinder finder = new FileSetWordFinder();

        // Assert
        Assert.Equal(DateTime.MinValue, finder.Duration);
    }

    [Fact]
    public void Duration_SetAndGet() {
        // Arrange
        FileSetWordFinder finder = new FileSetWordFinder();
        DateTime testTime = new DateTime(2024, 1, 15, 10, 30, 0);

        // Act
        finder.Duration = testTime;

        // Assert
        Assert.Equal(testTime, finder.Duration);
    }

    #endregion
}
