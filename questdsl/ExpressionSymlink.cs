using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public class ExpressionSymlink : Expression
    {
        public int ArgNumber;
        public string VarName;
        public ExpressionSymlink(int ArgNumber, string LocalVarName) 
        {
            this.ArgNumber = ArgNumber;
            this.VarName = LocalVarName;
        }

        public override bool SubStateModifies => false;

        public override string Compile()
        {
            throw new NotImplementedException();
        }
    }
}
