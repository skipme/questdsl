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
        public Transition(string name, bool IsItTrigger
            , List<Section> sections = null
            , Dictionary<int, ExpressionSymlink> symlinks = null)
            : base(name)
        {
            this.sections = sections ?? new List<Section>();
            IsTrigger = IsItTrigger;
            this.symlinks = symlinks ?? new Dictionary<int, ExpressionSymlink>();

            if (symlinks == null && !IsTrigger ||
                symlinks != null && IsTrigger)
                throw new Exception();

        }
        public Dictionary<int, ExpressionSymlink> symlinks;
        public List<Section> sections;
        public override bool SubStateModifies => IsChangedStates;

        public override string Compile()
        {
            throw new NotImplementedException();
        }


    }
}
