using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public class Dialogue : Transition
    {
        readonly bool IsChangedStates;

        public Dialogue(string name
            , List<Section> sections = null
            , Dictionary<int, ExpressionSymlink> symlinks = null)
            : base(name, false, sections, symlinks)
        {
        }

        public override bool SubStateModifies => IsChangedStates;

        public override string Compile()
        {
            throw new NotImplementedException();
        }
    }
}
