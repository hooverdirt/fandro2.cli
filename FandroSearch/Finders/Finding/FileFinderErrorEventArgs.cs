using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandroSearch.Finders.Finding {
    public class FileFinderErrorEventArgs {
        private string? message;
        private string? folderPath;

        public FileFinderErrorEventArgs() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="folderPath"></param>
        public FileFinderErrorEventArgs(string errorMessage, string? folderPath = null) : this() {
            this.message = errorMessage;
            this.folderPath = folderPath;
        }

        /// <summary>
        /// 
        /// </summary>
        public string? Message {
            get { return this.message; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string? FolderPath {
            get { return this.folderPath; }
        }
    }
}
