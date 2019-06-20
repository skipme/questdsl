using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public class Transition : State
    {
        public bool IsTrigger;
        readonly bool IsChangedStates;

        public Transition(string name, bool IsItTrigger, List<Section> sections, Dictionary<int, string> simlinks = null) : base(name)
        {
            sections = new List<Section>();
            IsTrigger = IsItTrigger;
            this.simlinks = simlinks;

            if (simlinks == null && !IsTrigger ||
                simlinks != null && IsTrigger)
                throw new Exception();

            this.sections = sections;
        }
        Dictionary<int, string> simlinks;
        public List<Section> sections;
        public override bool SubStateModifies => IsChangedStates;

        public override string Compile()
        {
            throw new NotImplementedException();
        }


    }
}
