using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    class Hinge
    {
        public Hinge(List<State> nodes)
        {
            this.AllNodes = nodes;

            States = new Dictionary<string, State>();
            Transitions = new Dictionary<string, Transition>();
            Triggers = new Dictionary<string, Transition>();

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] is Transition)
                {
                    if ((nodes[i] as Transition).IsTrigger)
                    {
                        Triggers.Add(nodes[i].Name, nodes[i] as Transition);
                    }
                    else
                    {
                        Transitions.Add(nodes[i].Name, nodes[i] as Transition);
                    }
                }
                else
                {
                    States.Add(nodes[i].Name, nodes[i]);
                }
            }
        }

        public List<State> AllNodes;
        public Dictionary<string, State> States;
        public Dictionary<string, Transition> Transitions;
        public Dictionary<string, Transition> Triggers;
    }
}
