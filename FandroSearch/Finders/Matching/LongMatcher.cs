using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandroSearch.Finders.Matching {
    public class LongMatcher : Matcher{
        /// <summary>
        /// // fileinfo data
        /// </summary>
        public long CurrentValue { get; set; }

        /// <summary>
        /// user value
        /// </summary>
        public long CompareValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool DoMatch() {
            bool ret = false;
            switch (this.MatcherAction) {
                case MatcherEnums.MatcherAction.Equals:
                    ret = CurrentValue == CompareValue;
                    break;
                case MatcherEnums.MatcherAction.NotEquals:
                    ret = CurrentValue != CompareValue;
                    break;
                case MatcherEnums.MatcherAction.Less:
                    ret = CurrentValue < CompareValue;
                    break;
                case MatcherEnums.MatcherAction.Greater:
                    ret = CurrentValue > CompareValue;
                    break;
            }

            return ret;
        }
    }
}
