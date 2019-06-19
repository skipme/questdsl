using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public abstract class Expression
    {
        // this is for decision making
        public abstract bool SubStateModifies { get; }
        public abstract string Compile();
    }
}
