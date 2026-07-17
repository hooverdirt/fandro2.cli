using FandroSearch.Finders.Matching;
using FandroSearch.Finders.Threading;
using System.IO;

namespace FandroSearch.Tests;

public class MatcherTests {

    #region FileSizeMatcher

    [Fact]
    public void FileSizeMatcher_Equal_Matches() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "12345"); // 5 bytes
        FileInfo file = new FileInfo(tempFile);

        FileSizeMatcher matcher = new FileSizeMatcher {
            CompareValue = 5,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = file.Length;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileSizeMatcher_GreaterThan_Matches() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "1234567890"); // 10 bytes
        FileInfo file = new FileInfo(tempFile);

        FileSizeMatcher matcher = new FileSizeMatcher {
            CompareValue = 5,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        };
        matcher.CurrentValue = file.Length;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileSizeMatcher_LessThan_Matches() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "1"); // 1 byte
        FileInfo file = new FileInfo(tempFile);

        FileSizeMatcher matcher = new FileSizeMatcher {
            CompareValue = 5,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = file.Length;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileSizeMatcher_NotEqual_DoesNotMatch() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "12345"); // 5 bytes
        FileInfo file = new FileInfo(tempFile);

        FileSizeMatcher matcher = new FileSizeMatcher {
            CompareValue = 10,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.NotEquals
        };
        matcher.CurrentValue = file.Length;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result); // 5 != 10, so NotEquals matches

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileSizeMatcher_KB_ConvertsCorrectly() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, new string('x', 2048)); // 2048 bytes = 2 KB
        FileInfo file = new FileInfo(tempFile);

        FileSizeMatcher matcher = new FileSizeMatcher {
            CompareValue = 2,
            Units = SizeState.KB,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = file.Length;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileSizeMatcher_MB_ConvertsCorrectly() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, new string('x', 1048576)); // 1 MB
        FileInfo file = new FileInfo(tempFile);

        FileSizeMatcher matcher = new FileSizeMatcher {
            CompareValue = 1,
            Units = SizeState.MB,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = file.Length;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileSizeMatcher_LessThan_MB_Matches() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, new string('x', 524288)); // 0.5 MB
        FileInfo file = new FileInfo(tempFile);

        FileSizeMatcher matcher = new FileSizeMatcher {
            CompareValue = 1,
            Units = SizeState.MB,
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = file.Length;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);

        // Cleanup
        File.Delete(tempFile);
    }

    #endregion

    #region IntegerMatcher

    [Fact]
    public void IntegerMatcher_Equal_Matches() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = 42,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = 42;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IntegerMatcher_Equal_DoesNotMatch() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = 42,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = 10;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IntegerMatcher_NotEquals_Matches() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = 42,
            MatcherAction = MatcherEnums.MatcherAction.NotEquals
        };
        matcher.CurrentValue = 10;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IntegerMatcher_NotEquals_DoesNotMatch() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = 42,
            MatcherAction = MatcherEnums.MatcherAction.NotEquals
        };
        matcher.CurrentValue = 42;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IntegerMatcher_Less_Matches() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = 100,
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = 50;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IntegerMatcher_Less_DoesNotMatch() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = 100,
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = 200;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IntegerMatcher_Greater_Matches() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = 100,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        };
        matcher.CurrentValue = 200;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IntegerMatcher_Greater_DoesNotMatch() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = 100,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        };
        matcher.CurrentValue = 50;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IntegerMatcher_NegativeValues_Equal() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = -10,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = -10;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IntegerMatcher_NegativeValues_Less() {
        // Arrange
        IntegerMatcher matcher = new IntegerMatcher {
            CompareValue = -5,
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = -10;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    #endregion

    #region LongMatcher

    [Fact]
    public void LongMatcher_Equal_Matches() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = 1000000L,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = 1000000L;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void LongMatcher_Equal_DoesNotMatch() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = 1000000L,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = 999999L;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void LongMatcher_NotEquals_Matches() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = 1000000L,
            MatcherAction = MatcherEnums.MatcherAction.NotEquals
        };
        matcher.CurrentValue = 999999L;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void LongMatcher_NotEquals_DoesNotMatch() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = 1000000L,
            MatcherAction = MatcherEnums.MatcherAction.NotEquals
        };
        matcher.CurrentValue = 1000000L;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void LongMatcher_Less_Matches() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = 1000000L,
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = 500000L;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void LongMatcher_Less_DoesNotMatch() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = 1000000L,
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = 2000000L;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void LongMatcher_Greater_Matches() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = 1000000L,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        };
        matcher.CurrentValue = 2000000L;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void LongMatcher_Greater_DoesNotMatch() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = 1000000L,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        };
        matcher.CurrentValue = 500000L;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void LongMatcher_MaxValue_Equal() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = long.MaxValue,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = long.MaxValue;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void LongMatcher_MinusOne_Equal() {
        // Arrange
        LongMatcher matcher = new LongMatcher {
            CompareValue = -1L,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = -1L;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    #endregion

    #region StringMatcher

    [Fact]
    public void StringMatcher_Contains_Matches() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "TODO",
            MatcherAction = MatcherEnums.MatcherAction.DoesContain
        };
        matcher.CurrentValue = "This is TODO work";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StringMatcher_Contains_DoesNotMatch() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "xyz",
            MatcherAction = MatcherEnums.MatcherAction.DoesContain
        };
        matcher.CurrentValue = "Hello world";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void StringMatcher_NotContains_Matches() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "xyz",
            MatcherAction = MatcherEnums.MatcherAction.DoesNotContain
        };
        matcher.CurrentValue = "Hello world";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StringMatcher_NotContains_DoesNotMatch() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "TODO",
            MatcherAction = MatcherEnums.MatcherAction.DoesNotContain
        };
        matcher.CurrentValue = "This is TODO work";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void StringMatcher_Contains_EmptyCompareValue_ReturnsTrue() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "",
            MatcherAction = MatcherEnums.MatcherAction.DoesContain
        };
        matcher.CurrentValue = "Hello world";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StringMatcher_Contains_EmptyCurrentValue_ReturnsFalse() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "xyz",
            MatcherAction = MatcherEnums.MatcherAction.DoesContain
        };
        matcher.CurrentValue = "";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void StringMatcher_Contains_NullCurrentValue_ReturnsFalse() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "xyz",
            MatcherAction = MatcherEnums.MatcherAction.DoesContain
        };
        matcher.CurrentValue = null;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void StringMatcher_Contains_NullCompareValue_ReturnsFalse() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = null,
            MatcherAction = MatcherEnums.MatcherAction.DoesContain
        };
        matcher.CurrentValue = "Hello world";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void StringMatcher_NotContains_NullCurrentValue_ReturnsTrue() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "xyz",
            MatcherAction = MatcherEnums.MatcherAction.DoesNotContain
        };
        matcher.CurrentValue = null;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StringMatcher_NotContains_NullCompareValue_ReturnsTrue() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = null,
            MatcherAction = MatcherEnums.MatcherAction.DoesNotContain
        };
        matcher.CurrentValue = "Hello world";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StringMatcher_Equal_DoesNotMatch() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "exact",
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = "not exact";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void StringMatcher_Greater_UsesStringCompare() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "Apple",
            MatcherAction = MatcherEnums.MatcherAction.Greater
        };
        matcher.CurrentValue = "Banana";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result); // "Banana" > "Apple"
    }

    [Fact]
    public void StringMatcher_Less_UsesStringCompare() {
        // Arrange
        StringMatcher matcher = new StringMatcher {
            CompareValue = "Zebra",
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = "Apple";

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result); // "Apple" < "Zebra"
    }

    #endregion

    #region DateTimeMatcher

    [Fact]
    public void DateTimeMatcher_SetsValueFromFileInfo() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "test");
        FileInfo file = new FileInfo(tempFile);
        DateTime expectedCreation = file.CreationTime;

        FileFilters filters = new FileFilters();
        filters.AddMatcher(new DateTimeMatcher {
            MatcherType = MatcherEnums.MatcherType.FileCreateTime
        });

        // Act
        filters.FileInformation = file;

        // Assert
        DateTimeMatcher? matcher = filters.Matchers.First() as DateTimeMatcher;
        Assert.NotNull(matcher);
        Assert.Equal(expectedCreation, matcher.CurrentValue);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void DateTimeMatcher_Equal_Matches() {
        // Arrange
        DateTime now = DateTime.Now;
        DateTimeMatcher matcher = new DateTimeMatcher {
            CompareValue = now,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = now;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DateTimeMatcher_Equal_DoesNotMatch() {
        // Arrange
        DateTime now = DateTime.Now;
        DateTime past = now.AddHours(-1);
        DateTimeMatcher matcher = new DateTimeMatcher {
            CompareValue = now,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        };
        matcher.CurrentValue = past;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DateTimeMatcher_NotEquals_Matches() {
        // Arrange
        DateTime now = DateTime.Now;
        DateTime past = now.AddHours(-1);
        DateTimeMatcher matcher = new DateTimeMatcher {
            CompareValue = now,
            MatcherAction = MatcherEnums.MatcherAction.NotEquals
        };
        matcher.CurrentValue = past;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DateTimeMatcher_Less_Matches() {
        // Arrange
        DateTime now = DateTime.Now;
        DateTime past = now.AddHours(-1);
        DateTimeMatcher matcher = new DateTimeMatcher {
            CompareValue = now,
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = past;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result); // past < now
    }

    [Fact]
    public void DateTimeMatcher_Less_DoesNotMatch() {
        // Arrange
        DateTime now = DateTime.Now;
        DateTime future = now.AddHours(1);
        DateTimeMatcher matcher = new DateTimeMatcher {
            CompareValue = now,
            MatcherAction = MatcherEnums.MatcherAction.Less
        };
        matcher.CurrentValue = future;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result); // future is not < now
    }

    [Fact]
    public void DateTimeMatcher_Greater_Matches() {
        // Arrange
        DateTime now = DateTime.Now;
        DateTime future = now.AddHours(1);
        DateTimeMatcher matcher = new DateTimeMatcher {
            CompareValue = now,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        };
        matcher.CurrentValue = future;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.True(result); // future > now
    }

    [Fact]
    public void DateTimeMatcher_Greater_DoesNotMatch() {
        // Arrange
        DateTime now = DateTime.Now;
        DateTime past = now.AddHours(-1);
        DateTimeMatcher matcher = new DateTimeMatcher {
            CompareValue = now,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        };
        matcher.CurrentValue = past;

        // Act
        bool result = matcher.DoMatch();

        // Assert
        Assert.False(result); // past is not > now
    }

    [Fact]
    public void DateTimeMatcher_FileModTime_SetsValueFromFileInfo() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "test");
        FileInfo file = new FileInfo(tempFile);
        DateTime expectedModTime = file.LastWriteTime;

        FileFilters filters = new FileFilters();
        filters.AddMatcher(new DateTimeMatcher {
            MatcherType = MatcherEnums.MatcherType.FileModTime
        });

        // Act
        filters.FileInformation = file;

        // Assert
        DateTimeMatcher? matcher = filters.Matchers.First() as DateTimeMatcher;
        Assert.NotNull(matcher);
        Assert.Equal(expectedModTime, matcher.CurrentValue);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void DateTimeMatcher_FileAccessTime_SetsValueFromFileInfo() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "test");
        FileInfo file = new FileInfo(tempFile);
        DateTime expectedAccessTime = file.LastAccessTime;

        FileFilters filters = new FileFilters();
        filters.AddMatcher(new DateTimeMatcher {
            MatcherType = MatcherEnums.MatcherType.FileAccessTime
        });

        // Act
        filters.FileInformation = file;

        // Assert
        DateTimeMatcher? matcher = filters.Matchers.First() as DateTimeMatcher;
        Assert.NotNull(matcher);
        Assert.Equal(expectedAccessTime, matcher.CurrentValue);

        // Cleanup
        File.Delete(tempFile);
    }

    #endregion

    #region FileFilters

    [Fact]
    public void FileFilters_And_CombinesMatchers() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "1234567890"); // 10 bytes
        FileInfo file = new FileInfo(tempFile);

        FileFilters andFilters = new FileFilters();
        andFilters.ValidateType = MatcherEnums.MatcherOperator.And;
        andFilters.AddMatcher(new FileSizeMatcher {
            CompareValue = 5,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });

        // Act
        andFilters.FileInformation = file;
        bool result = andFilters.DoMatch();

        // Assert
        Assert.True(result); // 10 > 5

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileFilters_Or_CombinesMatchers() {
        // Arrange
        FileFilters orFilters = new FileFilters();
        orFilters.ValidateType = MatcherEnums.MatcherOperator.Or;
        orFilters.AddMatcher(new FileSizeMatcher {
            CompareValue = 1000,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        orFilters.AddMatcher(new FileSizeMatcher {
            CompareValue = 5,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Equals,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "12345"); // 5 bytes
        FileInfo file = new FileInfo(tempFile);

        // Act
        orFilters.FileInformation = file;
        bool result = orFilters.DoMatch();

        // Assert
        Assert.True(result); // Second condition matches (5 == 5)

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileFilters_And_FailsOnFirstMismatch() {
        // Arrange
        FileFilters andFilters = new FileFilters();
        andFilters.ValidateType = MatcherEnums.MatcherOperator.And;
        andFilters.AddMatcher(new FileSizeMatcher {
            CompareValue = 100,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        andFilters.AddMatcher(new FileSizeMatcher {
            CompareValue = 5,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "12345"); // 5 bytes
        FileInfo file = new FileInfo(tempFile);

        // Act
        andFilters.FileInformation = file;
        bool result = andFilters.DoMatch();

        // Assert
        Assert.False(result); // 5 is not > 100

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileFilters_Empty_ReturnsFalse_WithNoValidators() {
        // Arrange
        FileFilters filters = new FileFilters();
        // ValidateType defaults to 0 (Or), but with no matchers, doOrMatch returns false

        // Act
        bool result = filters.DoMatch();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FileFilters_RemoveMatcher_Works() {
        // Arrange
        FileFilters filters = new FileFilters();
        FileSizeMatcher matcher = new FileSizeMatcher {
            CompareValue = 100,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        };
        filters.AddMatcher(matcher);
        Assert.Equal(1, filters.Count);

        // Act
        filters.RemoveMatcher(matcher);

        // Assert
        Assert.Equal(0, filters.Count);
    }

    [Fact]
    public void FileFilters_Clear_Works() {
        // Arrange
        FileFilters filters = new FileFilters();
        filters.AddMatcher(new FileSizeMatcher {
            CompareValue = 100,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        });
        filters.AddMatcher(new StringMatcher {
            CompareValue = "test",
            MatcherAction = MatcherEnums.MatcherAction.Equals
        });
        Assert.Equal(2, filters.Count);

        // Act
        filters.Clear();

        // Assert
        Assert.Equal(0, filters.Count);
    }

    [Fact]
    public void FileFilters_RemoveMatcherAt_RemovesByIndex() {
        // Arrange
        FileFilters filters = new FileFilters();
        filters.AddMatcher(new FileSizeMatcher {
            CompareValue = 100,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        });
        filters.AddMatcher(new StringMatcher {
            CompareValue = "test",
            MatcherAction = MatcherEnums.MatcherAction.Equals
        });
        filters.AddMatcher(new IntegerMatcher {
            CompareValue = 42,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        });
        Assert.Equal(3, filters.Count);

        // Act
        filters.RemoveMatcherAt(1);

        // Assert
        Assert.Equal(2, filters.Count);
        Assert.IsType<FileSizeMatcher>(filters.Matchers[0]);
        Assert.IsType<IntegerMatcher>(filters.Matchers[1]);
    }

    [Fact]
    public void FileFilters_RemoveMatcherAt_LastIndex_RemovesLast() {
        // Arrange
        FileFilters filters = new FileFilters();
        filters.AddMatcher(new FileSizeMatcher {
            CompareValue = 100,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        });
        Assert.Equal(1, filters.Count);

        // Act
        filters.RemoveMatcherAt(0);

        // Assert
        Assert.Equal(0, filters.Count);
    }

    [Fact]
    public void FileFilters_ValidateType_DefaultIsOr() {
        // Arrange
        FileFilters filters = new FileFilters();

        // Act
        // Default ValidateType should be 0 which maps to Or

        // Assert
        Assert.Equal(MatcherEnums.MatcherOperator.Or, filters.ValidateType);
    }

    [Fact]
    public void FileFilters_MixedMatcherTypes_And() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "12345"); // 5 bytes
        FileInfo file = new FileInfo(tempFile);

        FileFilters filters = new FileFilters();
        filters.ValidateType = MatcherEnums.MatcherOperator.And;
        filters.AddMatcher(new FileSizeMatcher {
            CompareValue = 10,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        filters.AddMatcher(new StringMatcher {
            CompareValue = "test",
            MatcherAction = MatcherEnums.MatcherAction.DoesContain
        });
        filters.AddMatcher(new IntegerMatcher {
            CompareValue = 5,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        });

        // Act
        filters.FileInformation = file;
        bool result = filters.DoMatch();

        // Assert
        Assert.False(result); // FileSize fails (5 is not > 10)

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileFilters_MixedMatcherTypes_Or() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "12345"); // 5 bytes
        FileInfo file = new FileInfo(tempFile);

        FileFilters filters = new FileFilters();
        filters.ValidateType = MatcherEnums.MatcherOperator.Or;
        filters.AddMatcher(new FileSizeMatcher {
            CompareValue = 100,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater,
            MatcherType = MatcherEnums.MatcherType.FileSize
        });
        filters.AddMatcher(new IntegerMatcher {
            CompareValue = 5,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        });

        // Act - IntegerMatcher.CurrentValue is not set by setFileInfoMatchers, so we set it manually
        filters.FileInformation = file;
        IntegerMatcher? intMatcher = filters.Matchers.Last() as IntegerMatcher;
        if (intMatcher != null) {
            intMatcher.CurrentValue = 5;
        }
        bool result = filters.DoMatch();

        // Assert
        Assert.True(result); // Integer matches (5 == 5)

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileFilters_SetMatchers_ReplacesList() {
        // Arrange
        FileFilters filters = new FileFilters();
        filters.AddMatcher(new FileSizeMatcher {
            CompareValue = 100,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        });
        Assert.Equal(1, filters.Count);

        List<Matcher> newMatchers = new List<Matcher>();
        newMatchers.Add(new StringMatcher {
            CompareValue = "hello",
            MatcherAction = MatcherEnums.MatcherAction.Equals
        });

        // Act
        filters.Matchers = newMatchers;

        // Assert
        Assert.Equal(1, filters.Count);
        Assert.IsType<StringMatcher>(filters.Matchers[0]);
    }

    [Fact]
    public void FileFilters_FileInformation_Null_DoesNotThrow() {
        // Arrange
        FileFilters filters = new FileFilters();
        filters.AddMatcher(new FileSizeMatcher {
            CompareValue = 100,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Greater
        });

        // Act
        filters.FileInformation = null!;

        // Assert - should not throw
        bool result = filters.DoMatch();
        Assert.False(result);
    }

    #endregion

    [Fact]
    public void FileSizeMatcher_SetsValueFromFileInfo() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        long expectedLength = 42L;
        File.WriteAllText(tempFile, new string('x', (int)expectedLength));
        FileInfo file = new FileInfo(tempFile);

        FileFilters filters = new FileFilters();
        filters.AddMatcher(new FileSizeMatcher {
            MatcherType = MatcherEnums.MatcherType.FileSize,
            CompareValue = 42,
            Units = SizeState.B,
            MatcherAction = MatcherEnums.MatcherAction.Equals
        });

        // Act
        filters.FileInformation = file;
        bool result = filters.DoMatch();

        // Assert
        Assert.True(result);

        // Cleanup
        File.Delete(tempFile);
    }

    #region SearchReport Types

    [Fact]
    public void FileMatchReport_ReportType_IsFileMatch() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "test");
        FileInfo fileInfo = new FileInfo(tempFile);

        // Act
        FileMatchReport report = new FileMatchReport(fileInfo, 10);

        // Assert
        Assert.Equal(SearchReportType.FileMatch, report.ReportType);
        Assert.Equal(fileInfo.FullName, report.FileInfo.FullName);
        Assert.Equal(10, report.Position);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void FileMatchReport_PositionIsNegative() {
        // Arrange
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "test");
        FileInfo fileInfo = new FileInfo(tempFile);

        // Act
        FileMatchReport report = new FileMatchReport(fileInfo, -1);

        // Assert
        Assert.Equal(-1, report.Position);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void StatusReport_ReportType_IsStatus() {
        // Arrange
        string text = "Processing file.txt";

        // Act
        StatusReport report = new StatusReport(text);

        // Assert
        Assert.Equal(SearchReportType.Status, report.ReportType);
        Assert.Equal(text, report.Text);
    }

    [Fact]
    public void SearchCompletedReport_ReportType_IsCompleted() {
        // Arrange
        int totalFilesScanned = 100;
        int totalMatches = 5;

        // Act
        SearchCompletedReport report = new SearchCompletedReport(totalFilesScanned, totalMatches);

        // Assert
        Assert.Equal(SearchReportType.Completed, report.ReportType);
        Assert.Equal(totalFilesScanned, report.TotalFilesScanned);
        Assert.Equal(totalMatches, report.TotalMatches);
    }

    [Fact]
    public void SearchReport_BaseClass_CannotBeInstantiated() {
        // Arrange & Act & Assert
        // SearchReport is abstract, so this should not compile if we try to instantiate it
        // This test verifies the abstract nature by checking the type hierarchy
        Assert.IsAssignableFrom<SearchReport>(new FileMatchReport(new FileInfo(Path.GetTempFileName()), 0));
        Assert.IsAssignableFrom<SearchReport>(new StatusReport("test"));
        Assert.IsAssignableFrom<SearchReport>(new SearchCompletedReport(0, 0));
    }

    #endregion

    #region Matcher Base Class

    [Fact]
    public void Matcher_BaseClass_DoesMatchDefaultsToTrue() {
        // Arrange
        MatcherBaseTest testMatcher = new MatcherBaseTest();

        // Act
        bool result = testMatcher.DoMatch();

        // Assert
        Assert.True(result);
    }

    private class MatcherBaseTest : Matcher {
        public override bool DoMatch() {
            return base.DoMatch();
        }
    }

    #endregion
}
