using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FandroSearch.Finders.Matching {
    public class IntegerMatcher : Matcher {
        /// <summary>
        /// // fileinfo data
        /// </summary>
        public int CurrentValue { get; set; }

        /// <summary>
        /// Value coming from the finder...
        /// </summary>
        public int CompareValue { get; set; }

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
