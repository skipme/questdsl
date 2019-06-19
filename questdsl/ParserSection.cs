using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public partial class Parser
    {
        public class ParserContext
        {
            public string CurrentNodeName;
            public bool HasSections;
            public List<string> AccumulatedExpressions;
            public bool SectionExecutionBody;
            public bool HasSubstateExpressions;
            public bool StringValueMultilineParsing;


            public Dictionary<int, string> simlinks = new Dictionary<int, string>();
            public SortedSet<string> simlinkReservedVars = new SortedSet<string>();
        }
        public ParserContext context;
        public Parser(string NodeName = null)
        {
            context = new ParserContext();
            context.CurrentNodeName = NodeName ?? "NONAME";
        }

        public void CloseParser()
        {

        }

        public void AppendLine(string line)
        {
            Dictionary<string, string> parsedParts = new Dictionary<string, string>();
            LineType lineType = EvaluateLineType(line, parsedParts);

            switch (lineType)
            {
                case LineType.simlink:
                    int definedArg = int.Parse(parsedParts["arg"]);
                    if (definedArg <= 0 || definedArg > 100)
                        throw new Exception();
                    if (context.simlinkReservedVars.Contains(parsedParts["sim"]))
                        throw new Exception();
                    if (context.simlinks.ContainsKey(definedArg))
                        throw new Exception();

                    context.simlinks.Add(definedArg, parsedParts["sim"]);
                    context.simlinkReservedVars.Add(parsedParts["sim"]);
                    break;
                case LineType.section_separator:
                    break;
                case LineType.substate_declaration:
                    if (context.simlinks.Count > 0)
                        throw new Exception();
                    break;
                case LineType.condition:
                    break;
                case LineType.executive:
                    break;
                case LineType.executive_invocation:
                    break;
                case LineType.comment:
                    break;
                case LineType.empty:
                    break;
                case LineType.undetermined:
                    break;
                default:
                    break;
            }
        }
    }
}