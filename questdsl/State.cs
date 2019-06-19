using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    class State
    {
        public string Name;
        public Dictionary<string, Substate> Substates;
    }
}
