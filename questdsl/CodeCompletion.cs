using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public partial class Parser
    {
        public class SuggestIssue
        {
            public string Name;
            public string Value;
        }
        readonly string[] controlSeq = new string[] { "----", "-->" };

        public IEnumerable<SuggestIssue> Suggest(IEnumerable<State> states, IEnumerable<Transition> transitions, string line)
        {
            // suggesting names:
            //                      localVar
            //                      state name
            //                      substate name (after (.))
            //                      transition name

            // structure control sequences:
            //                      ---- (after condition section part)
            //                      -->

            string prefix = "";
            if (string.IsNullOrWhiteSpace(line))
            {

            }
            //
            IEnumerable<SuggestIssue> varsc = from v in context.DefinedVars
                                              where v.StartsWith(prefix)
                                              select new SuggestIssue { Name = v + " (var)", Value = v };

            IEnumerable<SuggestIssue> controls = from v in controlSeq
                                                 where v.StartsWith(prefix)
                                                 select new SuggestIssue { Name = v, Value = v };
            return varsc.Concat(controls);
        }
    }
}
