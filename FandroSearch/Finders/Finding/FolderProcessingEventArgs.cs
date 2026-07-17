using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandroSearch.Finders.Finding {
    public class FolderProcessingEventArgs {
        private DirectoryInfo? dirinfo;
        private bool cancelled = false;
        
        /// <summary>
        /// 
        /// </summary>
        public FolderProcessingEventArgs() { }  

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public FolderProcessingEventArgs(DirectoryInfo info) : this() { 
            this.dirinfo = info;
        }    

        /// <summary>
        /// 
        /// </summary>
        public DirectoryInfo? DirectoryInfo {
            get { return this.dirinfo; }
            set { this.dirinfo = value;}
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Cancelled {
            get { return this.cancelled; }
            set { this.cancelled = value; }
        }
    }
}
