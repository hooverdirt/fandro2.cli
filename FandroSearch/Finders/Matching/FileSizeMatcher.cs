using FandroSearch.Finders.Finding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandroSearch.Finders.Matching {

    public enum SizeState {
        B,
        KB,
        MB,
        GB,
        TB
    }

    public class FileSizeMatcher  : LongMatcher {
        private SizeState units = SizeState.B;

        /// <summary>
        /// 
        /// </summary>
        public SizeState Units { 
            get {
                return this.units;
            }
            set {
                this.units = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual long convertCompareValueToUnit() {
            long actvalue = 0;

            switch(this.units) {
                case SizeState.B:
                    actvalue = this.CompareValue;
                    break;
                case SizeState.KB:
                    actvalue = this.CompareValue * 1024;
                    break;
                case SizeState.MB:
                    actvalue = this.CompareValue * (1024 * 1024);
                    break;
                case SizeState.GB:
                    actvalue = this.CompareValue * (1024 * 1024 * 1024);
                    break;  
                case SizeState.TB:
                    // um this is getting big to fit in an integer
                    // - we'll return 0 for now...
                    actvalue = this.CompareValue * (1024 * 1024 * 1024);
                    break;
            }

            return actvalue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool DoMatch() {
            bool ret = false;
            switch (this.MatcherAction) {
                case MatcherEnums.MatcherAction.Equals:
                    ret = CurrentValue == convertCompareValueToUnit();
                    break;
                case MatcherEnums.MatcherAction.NotEquals:
                    ret = CurrentValue != convertCompareValueToUnit();
                    break;
                case MatcherEnums.MatcherAction.Less:
                    ret = CurrentValue < convertCompareValueToUnit();
                    break;
                case MatcherEnums.MatcherAction.Greater:
                    ret = CurrentValue > convertCompareValueToUnit();
                    break;
            }

            return ret;
        }

    }
}
