using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FandroSearch.Finders.Matching {
    public class DateTimeMatcher : Matcher {
        // fileinfo data
        public DateTime CurrentValue { get; set; }

        // user data
        public DateTime CompareValue { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool DoMatch() {
            bool res = false;

            switch (this.MatcherAction) {
                case MatcherEnums.MatcherAction.Equals:
                    res = CurrentValue == CompareValue;
                    break;
                case MatcherEnums.MatcherAction.NotEquals:
                    res = CurrentValue != CompareValue;
                    break;
                case MatcherEnums.MatcherAction.Less:
                    res = CurrentValue < CompareValue;
                    break;
                case MatcherEnums.MatcherAction.Greater:
                    res = CurrentValue > CompareValue;
                    break;
            }

            return res;
        }
    }
}
