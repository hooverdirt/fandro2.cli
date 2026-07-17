using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandroSearch.Finders.Finding {
    public class FileFinder {
        private string startFolder = "";
        private List<String> startFolders = new List<String>();
        private string fileMask = "*";
        private string folderMask = "*";
        private EnumerationOptions? folderOptions = null;
        private EnumerationOptions? fileOptions = null;
        private bool recurse = false;
        private bool cancelProcessing = false;


        /// <summary>
        /// 
        /// </summary>
        private void setDefaultFolderOptions() {
            folderOptions = new EnumerationOptions {
                RecurseSubdirectories = false,
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = true
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void setDefaultFileOptions() {
            fileOptions = new EnumerationOptions {
                RecurseSubdirectories = false,
                IgnoreInaccessible = true
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mask"></param>
        private void setFileMask(string mask) {
            fileMask = mask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mask"></param>
        private void setFoldermask(string mask) {
            folderMask = mask;
        }

        /// <summary>
        /// 
        /// </summary>
        private void doExecute() {
            Queue<DirectoryInfo> directories = new Queue<DirectoryInfo>();

            if (this.startFolder.Contains(';')) {
                string[] strings = this.startFolder.Split(new char[] { ';' });
                foreach(string t in strings) {
                    directories.Enqueue(new DirectoryInfo(t));
                }
            }
            else {
                directories.Enqueue(new DirectoryInfo(this.startFolder));
            }

            // is this necessary: I'm pretty sure that the multiselection mode 
            // guarantees if a folder exists.... Singlemode not - but.... I'd
            // vote for taking this out...
            if (Directory.Exists(directories.Peek().FullName)) {
                IEnumerable<DirectoryInfo>? subdirectories = null;
                while (directories.Count > 0 && !cancelProcessing) {
                    DirectoryInfo currentdir = directories.Dequeue();

                    if (this.FolderProcessingEvent != null) {
                        FolderProcessingEventArgs args = new FolderProcessingEventArgs(currentdir);
                        this.FolderProcessingEvent(this, args);

                        if (args.Cancelled) {
                            cancelProcessing = true;
                            break;
                        }
                    }

                    try {
                        if (recurse) {
                            subdirectories = currentdir.EnumerateDirectories(this.folderMask,
                                new EnumerationOptions { RecurseSubdirectories = false, IgnoreInaccessible = true });
                        }

                        IEnumerable<FileInfo> files = currentdir.EnumerateFiles(fileMask, fileOptions ?? new EnumerationOptions());

                        foreach (FileInfo file in files) {
                            if (FileFoundEvent != null) {
                                FileFoundEventArgs arg = new FileFoundEventArgs(file);
                                FileFoundEvent(this, arg);

                                if (arg.Cancelled) {
                                    cancelProcessing = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception e) {
                        FileFinderErrorEventArgs errorArgs = new FileFinderErrorEventArgs(e.Message, currentdir.FullName);
                        ErrorOccurred?.Invoke(this, errorArgs);
                        continue;
                    }

                    // if subdirectories has no results AND we do recursion...
                    if (subdirectories != null && recurse && !cancelProcessing) {
                        // add directories
                        foreach (DirectoryInfo subdir in subdirectories) {
                            if (subdir.Name != "." || subdir.Name != "..") {
                                directories.Enqueue(subdir);
                            }

                            if (cancelProcessing) {
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FileFinder() {
            setDefaultFolderOptions();
            setDefaultFileOptions();
            setFoldermask("*");
            setFileMask("*");
        }


        /// <summary>
        /// 
        /// </summary>
        public string FolderMask {
            get { return folderMask; }
            set { folderMask = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string FileMask {
            get { return fileMask; }
            set { fileMask = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string TargetFolder {
            get { return startFolder; }
            set { startFolder = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Recursive {
            get { return recurse; }
            set { recurse = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Execute() {
            doExecute();
        }


        /// <summary>
        /// 
        /// </summary>
        public EnumerationOptions? FolderOptions {
            get { return folderOptions; }
            set { folderOptions = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public EnumerationOptions? FileOptions {
            get { return fileOptions; }
            set { fileOptions = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<FileFoundEventArgs>? FileFoundEvent;
        
        public event EventHandler<FolderProcessingEventArgs>? FolderProcessingEvent;

        public event EventHandler<FileFinderErrorEventArgs>? ErrorOccurred;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnFileFound(FileFoundEventArgs e) {
            EventHandler<FileFoundEventArgs>? handler = FileFoundEvent;
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnFolderProcessing(FolderProcessingEventArgs e) {
            EventHandler<FolderProcessingEventArgs>? handler = FolderProcessingEvent;
            if (handler!= null) {
                handler(this, e);
            }
        }
    }
}
