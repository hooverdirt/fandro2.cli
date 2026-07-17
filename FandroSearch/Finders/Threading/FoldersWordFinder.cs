using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FandroSearch.Finders.Finding;
using FandroSearch.Finders.Matching;

namespace FandroSearch.Finders.Threading {

    public class FoldersWordFinder : BaseModernWordFinder {
        private string startingfolder = String.Empty;
        private bool recursive = false;
        private FileFinder? findfiler = null;
        private int filesScanned = 0;
        private string? mask = String.Empty;

        /// <summary>
        /// Gets or sets whether the search is recursive.
        /// </summary>
        public bool Recursive {
            get { return this.recursive; }
            set { this.recursive = value; }
        }

        /// <summary>
        /// Gets or sets the file mask pattern.
        /// </summary>
        public string? Mask {
            get { return this.mask; }
            set { this.mask = value; }
        }

        /// <summary>
        /// Gets or sets the starting folder for the search.
        /// </summary>
        public string StartFolder {
            get { return this.startingfolder; }
            set { this.startingfolder = value; }
        }

        public FoldersWordFinder() {
        }

        /// <summary>
        /// Modern async version using IProgress&lt;T&gt; and CancellationToken.
        /// Pass an IProgress&lt;SearchReport&gt; to receive file matches, status updates, and completion.
        /// </summary>
        public override async Task DoWorkAsync(IProgress<SearchReport> progress, CancellationToken token) {
            await Task.Run(() => this.DoWorkInternal(progress, token), token);
        }

        /// <summary>
        /// Internal work implementation for folder-based searching.
        /// </summary>
        protected override void DoWorkInternal(IProgress<SearchReport> progress, CancellationToken token) {
            progress?.Report(new StatusReport(this.Duration.ToString("G")));
            this.Count = 0;
            this.filesScanned = 0;

            void Findfiler_FolderProcessingEvent(object? sender, FolderProcessingEventArgs e) {
                token.ThrowIfCancellationRequested();
                if (e.DirectoryInfo != null) {
                    progress?.Report(new StatusReport("Processing: " + e.DirectoryInfo.FullName));
                }
            }

            void Findfiler_ErrorOccurredEvent(object? sender, FileFinderErrorEventArgs e) {
                token.ThrowIfCancellationRequested();
                progress?.Report(new ErrorReport(e.Message ?? "Unknown error"));
            }

            void Findfiler_FileFoundEvent(object? sender, FileFoundEventArgs e) {
                this.filesScanned++;
                bool bconditions = true;

                token.ThrowIfCancellationRequested();

                if (this.Conditions != null && this.Conditions.Count > 0 && e.FileInfo != null) {
                    this.Conditions.FileInformation = e.FileInfo;
                    bconditions = this.Conditions.DoMatch();
                }

                token.ThrowIfCancellationRequested();

                ///progress?.Report(new StatusReport(e.FileInfo.FullName + " " + (bconditions ? "(match)" : "(no match)")));

                if (e.FileInfo != null && e.FileInfo.Length > 0 && bconditions == true) {
                    long position = -1;
                    bool searched = !String.IsNullOrEmpty(this.Pattern);
                    if (searched) {
                        position = this.FindTextPointersLong(this.Pattern, e.FileInfo);
                    }

                    // Report only when:
                    // 1) Pattern was set AND found (position > -1)
                    // 2) No pattern was set — just list files matching mask/conditions (searched == false)
                    // 3) Pattern was set but NOT found → do NOT report (position == -1 && searched == true)
                    if (!searched || position > -1) {
                        this.Count++;
                        progress?.Report(new FileMatchReport(e.FileInfo, position));
                    }
                }
            }

            findfiler = new FileFinder();
            try {
                findfiler.TargetFolder = startingfolder;
                findfiler.FileMask = mask ?? "*";
                findfiler.Recursive = recursive;
                findfiler.FileFoundEvent += Findfiler_FileFoundEvent;
                findfiler.FolderProcessingEvent += Findfiler_FolderProcessingEvent;
                findfiler.ErrorOccurred += Findfiler_ErrorOccurredEvent;
                findfiler.Execute();
            }
            finally {
                if (findfiler != null) {
                    findfiler.FileFoundEvent -= Findfiler_FileFoundEvent;
                    findfiler.FolderProcessingEvent -= Findfiler_FolderProcessingEvent;
                    findfiler.ErrorOccurred -= Findfiler_ErrorOccurredEvent;
                }
            }

            progress?.Report(new SearchCompletedReport(this.filesScanned, this.Count));
        }

        /// <summary>
        /// Pre-execution check for folder searching.
        /// </summary>
        protected override bool IsOKToContinue() {
            return this.startingfolder != null && this.mask != null;
        }
    }
}
