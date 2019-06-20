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
            public ParserContext(string NodeName)
            {
                this.NodeName = NodeName;
                StateNodeInstance = new State(NodeName);
            }
            public enum NodeType
            {
                undeclared,
                State,
                Trigger,
                Transition
            }
            public NodeType NodeDeclaredType = NodeType.undeclared;
            public readonly string NodeName;
            public State StateNodeInstance;

            public bool HasSections;
            public bool SectionExecutionBody;
            public bool DeclaredSubstatesByName;
            public bool DeclaredSubstatesByList;

            public string SubStateMultiline;

            public StringBuilder AccumulatedMultivar;

            public Dictionary<int, string> simlinks = new Dictionary<int, string>();
            public SortedSet<string> simlinkReservedVars = new SortedSet<string>();

            public void AddSimlink(int argnum, string name)
            {
                if (NodeDeclaredType != NodeType.Transition
                    && NodeDeclaredType != NodeType.undeclared)
                    throw new Exception();

                if (argnum <= 0 || argnum > 100)
                    throw new Exception();
                if (this.simlinkReservedVars.Contains(name))
                    throw new Exception();
                if (this.simlinks.ContainsKey(argnum))
                    throw new Exception();

                this.simlinks.Add(argnum, name);
                this.simlinkReservedVars.Add(name);

                NodeDeclaredType = NodeType.Transition;
            }
            public void DeclareSubstate(string substateName, ExpressionValue Value)
            {
                if (this.DeclaredSubstatesByList && SubStateMultiline != "$$$$$")
                    throw new Exception();

                if (substateName == null)
                {
                    if (SubStateMultiline == null)
                        throw new Exception();
                    substateName = SubStateMultiline;
                    SubStateMultiline = null;
                    if (Value == null)
                        throw new Exception();
                }
                if (Value != null)
                {
                    if (substateName == "$$$$$")
                    {
                        this.DeclareListedSubstate(Value);
                    }
                    else
                    {
                        StateNodeInstance.AddSubstate(substateName, Value);
                    }
                }
                else
                    SubStateMultiline = substateName;

                DeclaredSubstatesByName = true;
                NodeDeclaredType = NodeType.State;
            }
            public void DeclareListedSubstate(ExpressionValue Value)
            {
                if (Value == null)
                {
                    if (SubStateMultiline != null)
                        throw new Exception();
                    SubStateMultiline = "$$$$$";
                }
                else
                    StateNodeInstance.AddSubstate(null, Value);

                this.DeclaredSubstatesByList = true;
                NodeDeclaredType = NodeType.State;
            }
            public void AppendMultivar(string row)
            {
                if (AccumulatedMultivar == null)
                    AccumulatedMultivar = new StringBuilder();
                AccumulatedMultivar.AppendLine(row);
            }
            public string EndMultivar(string row)
            {
                AccumulatedMultivar.Append(row);
                string result = AccumulatedMultivar.ToString();
                AccumulatedMultivar = null;
                return result;
            }
            public bool IsInMultivar
            {
                get
                {
                    return AccumulatedMultivar != null;
                }
            }
            public void MakeTransition()
            {

            }
        }
        public ParserContext context;
        public Parser(string NodeName = null)
        {
            NodeName = NodeName ?? "NONAME";
            context = new ParserContext(NodeName);
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
                case LineType.simlink: // only for transitions
                    context.AddSimlink(int.Parse(parsedParts["arg"]), parsedParts["sim"]);
                    break;
                case LineType.section_separator:
                    break;
                case LineType.substate_declaration: // only for states
                    {
                        Dictionary<string, string> groups = new Dictionary<string, string>();
                        PartType pt = this.EvaluatePartType(parsedParts["value"], groups);
                        ExpressionValue value = null;
                        switch (pt)
                        {
                            case PartType.text_string:
                            case PartType.substate:
                            case PartType.substate_subVar:
                            case PartType.substate_stateVar:
                            case PartType.substate_allVar:
                                value = new ExpressionValue(ExpressionValue.ValueType.string_text, parsedParts["value"]);
                                break;
                            case PartType.digit:
                                value = new ExpressionValue(ExpressionValue.ValueType.string_text, null, null, int.Parse(groups["number"]));
                                break;
                            case PartType.text_multiline:
                                value = new ExpressionValue(ExpressionValue.ValueType.string_text, groups["string"]);
                                break;
                            case PartType.text_multiline_start:
                                if (context.IsInMultivar)
                                    throw new Exception();
                                context.AppendMultivar(groups["string"]);
                                break;
                            case PartType.text_multiline_end:
                                throw new Exception();
                                break;
                            default:
                                break;
                        }
                        context.DeclareSubstate(parsedParts["substate"], value);
                    }
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
                    if (context.IsInMultivar)
                    {
                        Dictionary<string, string> groups = new Dictionary<string, string>();
                        PartType pt = this.EvaluatePartType(line, groups);
                        if (pt != PartType.text_multiline_end)
                            context.AppendMultivar(line);
                        else
                        {
                            string lines = context.EndMultivar(groups["string"]);
                            if (context.SubStateMultiline != null)
                            {
                                context.DeclareSubstate(null, new ExpressionValue(ExpressionValue.ValueType.string_text, lines));
                            }
                            else throw new Exception();
                        }
                    }
                    else
                    {
                        Dictionary<string, string> groups = new Dictionary<string, string>();
                        PartType pt = this.EvaluatePartType(line, groups);
                        if (pt == PartType.text_multiline_start)
                        {
                            context.AppendMultivar(groups["string"]);
                            context.DeclareListedSubstate(null);
                            break;
                        }

                        if (context.DeclaredSubstatesByName)
                            throw new Exception();
                        if (pt == PartType.digit)
                            context.DeclareListedSubstate(new ExpressionValue(ExpressionValue.ValueType.number, null, null, int.Parse(line)));
                        else
                            context.DeclareListedSubstate(new ExpressionValue(ExpressionValue.ValueType.string_text, line));

                    }
                    break;
                default:
                    break;
            }
        }

        private ExpressionValue ParseValue(string v)
        {
            Dictionary<string, string> groups = new Dictionary<string, string>();
            PartType pt = this.EvaluatePartType(v, groups);
            if (pt == PartType.text_string
                || pt == PartType.text_multiline)
            {
                return new ExpressionValue(ExpressionValue.ValueType.string_text, groups["string"]);
            }

            if (pt == PartType.digit)
            {
                return new ExpressionValue(ExpressionValue.ValueType.string_text, null, null, int.Parse(groups["number"]));
            }

            return null;
        }
    }
}