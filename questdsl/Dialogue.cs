using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public class Dialogue : State
    {
        readonly bool IsChangedStates;

        public Dialogue(string name
            , List<Section> sections = null
            , Dictionary<int, ExpressionSymlink> symlinks = null)
            : base(name)
        {
            this.sections = sections ?? new List<Section>();
            this.symlinks = symlinks ?? new Dictionary<int, ExpressionSymlink>();
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
