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
                Sections = new List<Section>();
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

            public List<ExpressionBool> ProbesOr;
            public List<ExpressionExecutive> ExecBody;

            public bool HasSections;
            public bool SectionExecutionBody;
            public bool DeclaredSubstatesByName;
            public bool DeclaredSubstatesByList;

            public string SubStateMultiline;

            public StringBuilder AccumulatedMultivar;

            public Dictionary<int, string> simlinks = new Dictionary<int, string>();
            public SortedSet<string> simlinkReservedVars = new SortedSet<string>();
            private bool IsBodyWithoutConditionOccuredBefore;
            private List<Section> Sections;

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
            public void PushCondition(ExpressionBool expression)
            {
                if (NodeDeclaredType != NodeType.Transition
                    & NodeDeclaredType != NodeType.Trigger
                    & NodeDeclaredType != NodeType.undeclared)
                {
                    throw new Exception();
                }
                if (ExecBody != null)
                    throw new Exception();

                if (ProbesOr == null)
                    ProbesOr = new List<ExpressionBool>();

                ProbesOr.Add(expression);

                if (NodeDeclaredType == NodeType.undeclared)
                {
                    NodeDeclaredType = NodeType.Trigger;
                }
                this.SectionExecutionBody = false;
            }
            public void PushExec(ExpressionExecutive expression)
            {
                if (NodeDeclaredType != NodeType.Transition
                    & NodeDeclaredType != NodeType.Trigger
                    & NodeDeclaredType != NodeType.undeclared)
                {
                    throw new Exception();
                }
                if (HasSections && (ProbesOr == null || ProbesOr.Count == 0))
                    throw new Exception();
                if (!this.SectionExecutionBody && (HasSections || (ProbesOr != null && ProbesOr.Count > 0)))
                    throw new Exception();

                if (ExecBody == null)
                    ExecBody = new List<ExpressionExecutive>();

                ExecBody.Add(expression);

                if (NodeDeclaredType == NodeType.undeclared)
                {
                    NodeDeclaredType = NodeType.Trigger;
                }
            }
            public void MakeTransition()
            {

            }

            public void SectionSeparated()
            {
                if (ProbesOr == null || ProbesOr.Count == 0)
                    throw new Exception();
                if (this.SectionExecutionBody)
                    throw new Exception();

                this.SectionExecutionBody = true;
            }

            public void EmptyLineOccured()
            {
                if (!this.SectionExecutionBody && ProbesOr != null && ProbesOr.Count > 0)
                    throw new Exception();
                if (this.SectionExecutionBody && ExecBody != null && ExecBody.Count > 0)
                    throw new Exception();


                if (ProbesOr == null || ProbesOr.Count == 0)
                {
                    if (this.HasSections)
                        throw new Exception();
                }
                this.Sections.Add(new Section(ProbesOr, ExecBody));
                this.HasSections = true;
                ProbesOr = null;
                ExecBody = null;
                this.SectionExecutionBody = false;
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
                    context.SectionSeparated();
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
                    {
                        ExpressionBool.Operation op;
                        switch (parsedParts["cond"])
                        {
                            case "==":
                                op = ExpressionBool.Operation.eq;
                                break;
                            case "!=":
                                op = ExpressionBool.Operation.neq;
                                break;
                            case "<":
                                op = ExpressionBool.Operation.lt;
                                break;
                            case ">":
                                op = ExpressionBool.Operation.bt;
                                break;
                            case ">=":
                                op = ExpressionBool.Operation.bteq;
                                break;
                            case "<=":
                                op = ExpressionBool.Operation.lteq;
                                break;
                            default:
                                throw new Exception();
                                break;
                        }
                        ExpressionValue leftVal = ParseValue(parsedParts["left"]);
                        ExpressionValue rightVal = ParseValue(parsedParts["right"]);
                        ExpressionBool cond = new ExpressionBool(op, leftVal, rightVal);
                        context.PushCondition(cond);
                    }
                    break;
                case LineType.executive:
                    {
                        ExpressionExecutive.ExecuteType op;
                        switch (parsedParts["cond"])
                        {
                            case "+=":
                                op = ExpressionExecutive.ExecuteType.Add;
                                break;
                            case "=":
                                op = ExpressionExecutive.ExecuteType.Assign;
                                break;
                            case "--":
                                op = ExpressionExecutive.ExecuteType.Decrement;
                                break;
                            case "++":
                                op = ExpressionExecutive.ExecuteType.Increment;
                                break;
                            case "-=":
                                op = ExpressionExecutive.ExecuteType.Subtract;
                                break;
                            default:
                                throw new Exception();
                                break;
                        }
                        ExpressionValue leftVal = ParseValue(parsedParts["left"]);
                        ExpressionValue rightVal = ParseValue(parsedParts["right"]);
                        ExpressionExecutive cond = new ExpressionExecutive(op, leftVal, rightVal);
                        context.PushExec(cond);
                    }
                    break;
                case LineType.executive_invocation:
                    break;
                case LineType.comment:
                    break;
                case LineType.empty:
                    context.EmptyLineOccured();
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

        public ExpressionValue ParseValue(string v)
        {
            Dictionary<string, string> groups = new Dictionary<string, string>();
            PartType pt = this.EvaluatePartType(v, groups);

            switch (pt)
            {
                case PartType.substate:
                    return new ExpressionValue(ExpressionValue.ValueType.SubstateName, groups["left"], groups["right"]);
                    break;
                case PartType.substate_subVar:
                    return new ExpressionValue(ExpressionValue.ValueType.StateName_SubstateRef, groups["left"], groups["right"]);
                    break;
                case PartType.substate_stateVar:
                    return new ExpressionValue(ExpressionValue.ValueType.StateRef_SubstateName, groups["left"], groups["right"]);
                    break;
                case PartType.substate_allVar:
                    return new ExpressionValue(ExpressionValue.ValueType.StateRef_SubstateRef, groups["left"], groups["right"]);
                    break;
                case PartType.digit:
                    return new ExpressionValue(ExpressionValue.ValueType.number, null, null, int.Parse(groups["number"]));
                    break;
                case PartType.text_string:
                    if (v.Trim() == "null")
                        return new ExpressionValue(ExpressionValue.ValueType.Reference, "null");
                    else
                        return new ExpressionValue(ExpressionValue.ValueType.string_text, v);
                    break;
                case PartType.text_multiline:
                    return new ExpressionValue(ExpressionValue.ValueType.string_text, groups["string"]);
                    break;
                case PartType.variable:
                    return new ExpressionValue(ExpressionValue.ValueType.Reference, groups["var"]);
                    break;
                case PartType.text_multiline_start:
                case PartType.text_multiline_end:
                default:
                    throw new Exception();
                    break;
            }


            return null;
        }
    }
}