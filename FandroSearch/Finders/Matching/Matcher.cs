using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FandroSearch.Finders.Matching {
    public abstract class Matcher {
        public MatcherEnums.MatcherAction MatcherAction { get; set; }
        public MatcherEnums.MatcherType MatcherType { get; set; }
        public virtual bool DoMatch() { return true; }
    }
}
