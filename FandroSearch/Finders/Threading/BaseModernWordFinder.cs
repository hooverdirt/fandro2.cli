using FandroSearch.Finders.Matching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FandroSearch.Finders.Threading {
    public class BaseModernWordFinder {
        private string pattern = null!;
        private bool casesensitive = false;
        private DateTime duration = DateTime.MinValue;
        private FileFilters conditions = new FileFilters();
        private int count = 0;

        /// <summary>
        /// Gets or sets the search pattern.
        /// </summary>
        public string Pattern {
            get { return this.pattern; }
            set { this.pattern = value; }
        }

        /// <summary>
        /// Gets or sets whether the search is case-sensitive.
        /// </summary>
        public bool CaseSensitive {
            get { return this.casesensitive; }
            set { this.casesensitive = value; }
        }

        /// <summary>
        /// Gets or sets the duration/timestamp for the search.
        /// </summary>
        public DateTime Duration {
            get { return this.duration; }
            set { this.duration = value; }
        }

        /// <summary>
        /// Gets or sets the file conditions/filters.
        /// </summary>
        public FileFilters Conditions {
            get { return this.conditions; }
            set { this.conditions = value; }
        }

        /// <summary>
        /// Gets or sets the match count.
        /// </summary>
        public int Count {
            get { return this.count; }
            set { this.count = value; }
        }

        /// <summary>
        /// Modern async entry point for search work. Override in derived classes.
        /// </summary>
        public virtual Task DoWorkAsync(IProgress<SearchReport> progress, CancellationToken token) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Internal work implementation. Override in derived classes.
        /// </summary>
        protected virtual void DoWorkInternal(IProgress<SearchReport> progress, CancellationToken token) {
        }

        /// <summary>
        /// Pre-execution check. Override in derived classes to add validation.
        /// </summary>
        protected virtual bool IsOKToContinue() {
            return true;
        }

        /// <summary>
        /// Boyer-Moore-Horspool search using memory-mapped file pointers.
        /// </summary>
        unsafe protected virtual long BoyerMooreHorspoolPointersLong(string text, MemoryMappedViewAccessor accessor, long size) {
            long ret = 0;

            if (!this.casesensitive) {
                this.pattern = this.pattern.ToUpper();
            }

            byte* ptrMemMap = (byte*)0;

            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptrMemMap);

            int[] bad_shift = new int[char.MaxValue + 1];

            for (int i = 0; i < char.MaxValue + 1; i++) {
                bad_shift[i] = this.pattern.Length;
            }

            int last = this.pattern.Length - 1;

            for (int i = 0; i < last; i++) {
                bad_shift[this.pattern[i]] = last - i;
            }

            int patlength = this.pattern.Length;
            long pos = patlength - 1;
            char lastchar;
            int numskip = 0;
            bool found = false;

            if (pos != 0) {
                ret = -1;
                lastchar = this.pattern[patlength - 1];
                while (pos < size) {
                    char tmpchar = '0';

                    if (!this.casesensitive) {
                        tmpchar = char.ToUpper(Convert.ToChar(ptrMemMap[pos]));
                    }
                    else {
                        tmpchar = Convert.ToChar(ptrMemMap[pos]);
                    }

                    if (tmpchar != lastchar) {
                        numskip = bad_shift[tmpchar];
                    }
                    else {
                        int i = patlength - 1;
                        found = true;
                        numskip = patlength;
                        while (i > 0) {
                            pos--;

                            if (!this.casesensitive) {
                                tmpchar = char.ToUpper(Convert.ToChar(ptrMemMap[pos]));
                            }
                            else {
                                tmpchar = Convert.ToChar(ptrMemMap[pos]);
                            }

                            if (tmpchar != this.pattern[i - 1]) {
                                found = false;
                                numskip = patlength - i + bad_shift[lastchar];
                                break;
                            }
                            i--;
                        }
                    }

                    if (found) {
                        ret = pos;
                        return ret;
                    }
                    pos += numskip;
                }
            }

            return ret;
        }

        /// <summary>
        /// Helper to search a file using memory-mapped Boyer-Moore-Horspool.
        /// </summary>
        protected virtual long FindTextPointersLong(string text, FileInfo file) {
            long ret = -1;

            try {
                using (MemoryMappedFile memfile = MemoryMappedFile.CreateFromFile(
                      file.FullName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read)) {
                    try {
                        using (MemoryMappedViewAccessor accessor = memfile.CreateViewAccessor(
                            0, file.Length, MemoryMappedFileAccess.Read)) {
                            ret = this.BoyerMooreHorspoolPointersLong(text, accessor, file.Length);
                        }
                    }
                    catch (Exception ex) {
                        Trace.TraceError(ex.Message + " -- " + file.FullName + " - " + file.Length);
                    }
                }
            }
            catch (Exception ex) {
                Trace.TraceError(ex.Message + " -- " + file.FullName + " - " + file.Length);
            }

            return ret;
        }
    }
}
