using FandroSearch.Finders.Matching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FandroSearch.Finders.Threading {
    [Obsolete("Use BaseModernWordFinder instead")]
    public class BaseFileWordFinder {
        private string pattern = null!;
        private bool casesensitive = false;
        private DateTime duration = DateTime.MinValue;
        private FileFilters conditions = new FileFilters();
        private Thread nthread = null!;
        private int count = 0;
        private ManualResetEvent stopThread = new ManualResetEvent(false);
        private ManualResetEvent threadHasStopped = new ManualResetEvent(false);

        /// <summary>
        /// 
        /// </summary>
        public ManualResetEvent StopWorkResetEvent { 
            get { return stopThread; } 
            set { stopThread = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ManualResetEvent FinishedWorkResetEvent {
            get { return this.threadHasStopped; }
            set { this.threadHasStopped = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Pattern {
            get { return this.pattern; }
            set { this.pattern = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CaseSensitive {
            get { return this.casesensitive; }
            set { this.casesensitive = value; } 
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Duration {
           get { return this.duration; }
           set { this.duration = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public FileFilters Conditions {
            get { return this.conditions; }
            set { this.conditions = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Thread CurrentThread {
            get { return this.nthread; }
            set { this.nthread = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count {
            get { return this.count; }
            set { this.count = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsOKToContinue() {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="accessor"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        unsafe virtual protected long BoyerMooreHorspoolPointersLong(string text, MemoryMappedViewAccessor accessor, long size) {
            long ret = 0;

            if (!casesensitive) {
                pattern = pattern.ToUpper();
            }

            byte* ptrMemMap = (byte*)0;

            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptrMemMap);


            int[] bad_shift = new int[char.MaxValue + 1];


            for (int i = 0; i < char.MaxValue + 1; i++) {
                bad_shift[i] = pattern.Length;
            }

            int last = pattern.Length - 1;

            for (int i = 0; i < last; i++) {
                bad_shift[pattern[i]] = last - i;
            }

            int patlength = pattern.Length;
            long pos = patlength - 1;
            char lastchar;
            int numskip = 0;
            bool found = false;

            if (pos != 0) {
                ret = -1;
                lastchar = pattern[patlength - 1];
                while (pos < size) {

                    if (stopThread.WaitOne(0, true)) {
                        ret = -666;
                        break;
                    }
                    char tmpchar = '0';

                    if (!casesensitive) {
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

                            if (!casesensitive) {
                                tmpchar = char.ToUpper(Convert.ToChar(ptrMemMap[pos]));
                            }
                            else {
                                tmpchar = Convert.ToChar(ptrMemMap[pos]);
                            }

                            if (tmpchar != pattern[i - 1]) {
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
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual long FindTextPointersLong(string text, FileInfo file) {
            long ret = -1;

            try {
                using (MemoryMappedFile memfile = MemoryMappedFile.CreateFromFile(
                      file.FullName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read)) {
                    try {
                        using (MemoryMappedViewAccessor accessor = memfile.CreateViewAccessor(
                            0, file.Length, MemoryMappedFileAccess.Read)) {
                            // do your fancy Boyer-Moore-Horspool search against the memory mapped file/pointers
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

        /// <summary>
        /// Override all work here....
        /// </summary>
        protected virtual void DoWork() {

        }

        /// <summary>
        /// 
        /// </summary>
        public BaseFileWordFinder() {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Execute() {
            if (this.IsOKToContinue()) {
                this.nthread = new Thread(DoWork);
                nthread.Start();
                nthread.Name = "Fandro2_fileset_" + Guid.NewGuid();
                duration = DateTime.Now;
                nthread.Start();
            }
        }
    }
}
