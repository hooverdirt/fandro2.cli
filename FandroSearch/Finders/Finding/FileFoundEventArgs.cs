using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandroSearch.Finders.Finding {
    public delegate void FileFoundEventHandler(object sender, FileFoundEventArgs e);

    public class FileFoundEventArgs {
        private bool cancelledfinding = false;

        public FileFoundEventArgs() {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public FileFoundEventArgs(FileInfo file) : this() {
            FileInfo = file;
        }

        /// <summary>
        /// 
        /// </summary>
        public FileInfo? FileInfo { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool Cancelled {
            get { return cancelledfinding; }
            set { cancelledfinding = value; }
        }
    }
}
