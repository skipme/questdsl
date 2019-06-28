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
            this.AllNodesDict = new Dictionary<string, State>();

            for (int i = 0; i < nodes.Count; i++)
            {
                AddNode(nodes[i]);
            }
        }
        public void RemoveNode(string name)
        {
            if (!AllNodesDict.ContainsKey(name))
                throw new Exception();

            State node = AllNodesDict[name];
            AllNodesDict.Remove(name);
            AllNodes.Remove(node);
        }
        public void ReplaceNode(State a, State b)
        {
            this.RemoveNode(a.Name);
            this.AddNode(b);
        }
        public void RenameNode(string name, string newName)
        {
            if (AllNodesDict.ContainsKey(newName) || !AllNodesDict.ContainsKey(name))
                throw new Exception();

            AllNodesDict.Remove(name);
            State node = AllNodesDict[name];
            AllNodesDict.Add(node.Name, node);
        }
        public State GetNode(string name)
        {
            if (!AllNodesDict.ContainsKey(name))
                throw new Exception();
            return AllNodesDict[name];
        }
        public string AddNode(State node)
        {
            if (AllNodesDict.ContainsKey(node.Name))
                throw new Exception();

            AllNodesDict.Add(node.Name, node);
            AllNodes.Add(node);
            return node.Name;
        }

        public List<State> AllNodes;
        public Dictionary<string, State> AllNodesDict;

        public IEnumerable<State> GetStates()
        {
            return from s in AllNodes
                   where s is State
                   select s;
        }
        public IEnumerable<Transition> GetTransitions()
        {
            return from s in AllNodes
                   where s is Transition && !(s as Transition).IsTrigger
                   select s as Transition;
        }
        public IEnumerable<Transition> GetTriggers()
        {
            return from s in AllNodes
                   where s is Transition && (s as Transition).IsTrigger
                   select s as Transition;
        }

    }
}
