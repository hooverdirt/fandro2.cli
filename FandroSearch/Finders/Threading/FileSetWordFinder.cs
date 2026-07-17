using FandroSearch.Finders.Matching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FandroSearch.Finders.Threading {

    public class FileSetWordFinder : BaseModernWordFinder {
        private List<string> fileset = new List<string>();

        public FileSetWordFinder() { }

        /// <summary>
        /// Gets or sets the list of file paths to search.
        /// </summary>
        public List<string> Fileset { get => fileset; set => fileset = value; }

        /// <summary>
        /// Modern async version using IProgress&lt;T&gt; and CancellationToken.
        /// Pass an IProgress&lt;SearchReport&gt; to receive file matches, status updates, and completion.
        /// </summary>
        public override async Task DoWorkAsync(IProgress<SearchReport> progress, CancellationToken token) {
            await Task.Run(() => this.DoWorkInternal(progress, token), token);
        }

        /// <summary>
        /// Internal work implementation for file set searching.
        /// </summary>
        protected override void DoWorkInternal(IProgress<SearchReport> progress, CancellationToken token) {
            progress?.Report(new StatusReport(this.Duration.ToString("G")));
            int scanned = 0;

            foreach (string s in this.fileset) {
                token.ThrowIfCancellationRequested();
                scanned++;

                if (File.Exists(s)) {
                    FileInfo p = new FileInfo(s);
                    bool bconditions = true;

                    if (this.Conditions != null && this.Conditions.Count > 0) {
                        this.Conditions.FileInformation = p;
                        bconditions = this.Conditions.DoMatch();
                    }

                    if (p.Length > 0 && bconditions == true) {
                        if (!String.IsNullOrEmpty(this.Pattern)) {
                            long position = this.FindTextPointersLong(this.Pattern, p);
                            if (position > -1) {
                                this.Count++;
                                progress?.Report(new FileMatchReport(p, position));
                            }
                        }
                    }
                }
            }

            progress?.Report(new SearchCompletedReport(scanned, this.Count));
        }

        /// <summary>
        /// Pre-execution check for file set searching.
        /// </summary>
        protected override bool IsOKToContinue() {
            return this.Pattern != null && this.fileset.Count > 0;
        }
    }
}
