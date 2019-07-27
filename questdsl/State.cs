using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public class State : Expression
    {
        public State(string name)
        {
            this.Name = name;
            SubstatesBook = new Dictionary<string, ExpressionSubStateDefinition>();
            Substates = new List<ExpressionSubStateDefinition>();
        }

        public void AddSubstate(string name, ExpressionValue defaultValue)
        {
            if (name == null)
                name = ($"@{Guid.NewGuid().ToString("D").Substring(0, 8)}");

            ExpressionSubStateDefinition sub = new ExpressionSubStateDefinition(this.Name, name, defaultValue);
            if (SubstatesBook.ContainsKey(name))
                throw new Exception();

            Substates.Add(sub);
            SubstatesBook.Add(name, sub);
        }
        public void RemoveSubstate(string name)
        {
            ExpressionSubStateDefinition sub = SubstatesBook[name];
            Substates.Remove(sub);
            SubstatesBook.Remove(name);
        }

        public override string Compile()
        {
            throw new NotImplementedException();
        }

        public string Name;
        public List<ExpressionSubStateDefinition> Substates;
        public Dictionary<string, ExpressionSubStateDefinition> SubstatesBook;

        public override bool SubStateModifies => false;
    }
}
