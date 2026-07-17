using System;
using System.IO;

namespace FandroSearch.Finders.Threading {
    /// <summary>
    /// Base class for search progress reports.
    /// </summary>
    public abstract class SearchReport {
        public SearchReportType ReportType { get; protected set; }
    }

    /// <summary>
    /// Reports a file match found during search.
    /// </summary>
    public class FileMatchReport : SearchReport {
        public FileInfo FileInfo { get; }
        public long Position { get; }

        public FileMatchReport(FileInfo fileInfo, long position) {
            FileInfo = fileInfo;
            Position = position;
            ReportType = SearchReportType.FileMatch;
        }
    }

    /// <summary>
    /// Reports a status update during search.
    /// </summary>
    public class StatusReport : SearchReport {
        public string Text { get; }

        public StatusReport(string text) {
            Text = text;
            ReportType = SearchReportType.Status;
        }
    }

    /// <summary>
    /// Reports an error that occurred during search.
    /// </summary>
    public class ErrorReport : SearchReport {
        public string Message { get; }

        public ErrorReport(string message) {
            Message = message;
            ReportType = SearchReportType.Error;
        }
    }

    /// <summary>
    /// Reports search completion with final counts.
    /// </summary>
    public class SearchCompletedReport : SearchReport {
        public int TotalFilesScanned { get; }
        public int TotalMatches { get; }

        public SearchCompletedReport(int totalFilesScanned, int totalMatches) {
            TotalFilesScanned = totalFilesScanned;
            TotalMatches = totalMatches;
            ReportType = SearchReportType.Completed;
        }
    }
}
