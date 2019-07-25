using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public partial class Parser
    {
        public static State ParseNode(string name, string codeText)
        {
            Parser p = new Parser(name);
            string[] lines = codeText.Split(
                 new[] { "\r\n", "\r", "\n" },
                 StringSplitOptions.None
             );
            for (int i = 0; i < lines.Length; i++)
            {
                //try
                //{
                p.AppendLine(lines[i], i);
                //}
                //catch (Exception e)
                //{
                //    throw new Exception("at line " + i, e);
                //}
            }
            return p.Product();
        }
        public ParserContext context;
        public Parser(string NodeName = null)
        {
            NodeName = NodeName ?? "NONAME";
            context = new ParserContext(NodeName);
        }

        public State Product()
        {
            if (context.NodeDeclaredType == ParserContext.NodeType.undeclared)
                throw new Exception();

            this.context.EmptyLineOccured();

            switch (context.NodeDeclaredType)
            {
                case ParserContext.NodeType.State:
                    return context.StateNodeInstance;
                case ParserContext.NodeType.Trigger:
                    return new Transition(context.NodeName, true, context.Sections);
                case ParserContext.NodeType.Transition:
                    return new Transition(context.NodeName, false, context.Sections, context.symlinks);
                case ParserContext.NodeType.Dialogue:
                    return new Dialogue(context.NodeName, context.Sections, context.symlinks);
                default:
                    throw new Exception();
                    break;
            }

        }
        public void AppendLine(string line, int lineNumber)
        {
            context.CurrentLineNumber = lineNumber;
            AppendLine(line);
        }
        public void AppendLine(string line)
        {
            Dictionary<string, string> parsedParts = new Dictionary<string, string>();
            LineType lineType = EvaluateLineType(line, parsedParts);

            switch (lineType)
            {
                case LineType.symlink: // only for transitions and dialogs
                    context.AddSymlink(int.Parse(parsedParts["arg"]), parsedParts["sym"]);
                    break;
                case LineType.section_separator:
                    context.SectionSeparated();
                    break;
                case LineType.dialogue_say:
                    if (context.NodeDeclaredType != ParserContext.NodeType.State)
                    {
                        context.NodeDeclaredType = ParserContext.NodeType.Dialogue;
                    }
                    else
                    {
                        throw new Exception();
                    }

                    if ("$$$$$" == parsedParts["characterName"])
                        throw new Exception();

                    Dictionary<string, string> textgroups = new Dictionary<string, string>();
                    PartType ptext = this.EvaluatePartType(parsedParts["text"], textgroups);
                    ExpressionValue textvalue = null;
                    switch (ptext)
                    {
                        case PartType.text_string:
                        case PartType.substate:
                        case PartType.substate_subVar:
                        case PartType.substate_stateVar:
                        case PartType.substate_allVar:
                            textvalue = new ExpressionValue(ExpressionValue.ValueType.string_text, parsedParts["text"]);
                            break;
                        case PartType.digit:
                            textvalue = new ExpressionValue(ExpressionValue.ValueType.number, null, null, int.Parse(textgroups["number"]));
                            break;
                        case PartType.text_multiline:
                            textvalue = new ExpressionValue(ExpressionValue.ValueType.string_text, textgroups["string"]);
                            break;
                        case PartType.text_multiline_start:
                            if (context.IsInMultivar)
                                throw new Exception();
                            context.AppendMultivar(textgroups["string"]);
                            break;
                        case PartType.text_multiline_end:
                            throw new Exception();
                            break;
                        default:
                            break;
                    }
                    if(ptext == PartType.text_multiline_start)
                    {
                        context.SayMultiline = parsedParts["characterName"];
                        break;
                    }
                    ExpressionExecutive exec = new ExpressionExecutive("say", new List<ExpressionValue>()
                    {
                        ParseValue(parsedParts["characterName"]),
                        textvalue,
                    });
                    context.PushExec(exec);
                    break;
                case LineType.substate_declaration: // only for states
                    {
                        if ("$$$$$" == parsedParts["substate"])
                            throw new Exception();

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
                                value = new ExpressionValue(ExpressionValue.ValueType.number, null, null, int.Parse(groups["number"]));
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
                    {
                        List<string> args = SplitArgs(parsedParts["args"]);
                        List<ExpressionValue> argsv = (from a in args
                                                       select ParseValue(a)).ToList();
                        ExpressionExecutive cond = new ExpressionExecutive(parsedParts["transName"], argsv);
                        context.PushExec(cond);
                    }
                    break;
                case LineType.executive_assign_op:
                    {
                        ExpressionExecutive.ExecuteType op;
                        switch (parsedParts["op"])
                        {
                            case "+":
                                op = ExpressionExecutive.ExecuteType.AssignAdd;
                                break;
                            case "-":
                                op = ExpressionExecutive.ExecuteType.AssignSub;
                                break;
                            case "*":
                                op = ExpressionExecutive.ExecuteType.AssignMul;
                                break;
                            case "/":
                                op = ExpressionExecutive.ExecuteType.AssignDiv;
                                break;
                            case "%":
                                op = ExpressionExecutive.ExecuteType.AssignModulo;
                                break;
                            default:
                                throw new Exception();
                                break;
                        }
                        ExpressionValue assignVar = ParseValue(parsedParts["var"]);

                        ExpressionValue leftVal = ParseValue(parsedParts["left"]);
                        ExpressionValue rightVal = ParseValue(parsedParts["right"]);
                        ExpressionExecutive cond = new ExpressionExecutive(assignVar, op, leftVal, rightVal);
                        context.PushExec(cond);
                    }
                    break;
                case LineType.comment:
                    break;
                case LineType.empty:
                    context.EmptyLineOccured();
                    break;
                case LineType.undetermined:
                    // TODO: replace multiline logic - line can detected as any linetype not only undetermined
                    if (context.IsInMultivar)
                    {
                        Dictionary<string, string> groups = new Dictionary<string, string>();
                        PartType pt = this.EvaluatePartType(line, groups);
                        if (pt != PartType.text_multiline_end)
                            context.AppendMultivar(line);
                        else
                        {
                            string lines = context.EndMultivar(groups["string"]);
                            if (context.SubStateMultiline == null && context.NodeDeclaredType == ParserContext.NodeType.State)
                            {
                                throw new Exception();
                            }
                            if (context.SayMultiline == null && context.NodeDeclaredType == ParserContext.NodeType.Dialogue)
                            {
                                throw new Exception();
                            }
                            if (context.NodeDeclaredType == ParserContext.NodeType.State)
                            {
                                context.DeclareSubstate(null, new ExpressionValue(ExpressionValue.ValueType.string_text,
                                    lines));
                            }
                            else if (context.NodeDeclaredType == ParserContext.NodeType.Dialogue)
                            {

                                ExpressionExecutive execsay = new ExpressionExecutive("say", new List<ExpressionValue>()
                                {
                                    ParseValue(context.SayMultiline), new ExpressionValue(ExpressionValue.ValueType.string_text,
                                    lines)
                                });
                                context.SayMultiline = null;
                                context.PushExec(execsay);
                            }
                        }
                    }
                    else
                    {
                        {
                            string declaration = line.Trim().ToLower();
                            {
                                if (declaration == "trans" || declaration == "transition")
                                {
                                    if (context.NodeDeclaredType != ParserContext.NodeType.undeclared)
                                        throw new Exception();
                                    context.NodeDeclaredType = ParserContext.NodeType.Transition;
                                    break;
                                }
                            }
                        }

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
        public List<string> SplitArgs(string argsPart)
        {
            List<string> result = new List<string>();
            bool instring = false;
            bool backslash = false;
            StringBuilder current = new StringBuilder();
            for (int i = 0; i < argsPart.Length; i++)
            {
                if (instring && argsPart[i] == '\\')
                {
                    backslash = true;
                    continue;
                }

                if (!backslash && argsPart[i] == '"')
                {
                    instring = !instring;
                    if (!instring && i + 1 < argsPart.Length)
                    {
                        if (argsPart[i + 1] != ' ')
                            throw new Exception();
                    }
                    continue;
                }
                if (!instring && argsPart[i] == ' ')
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }
                if (backslash)
                {
                    switch (argsPart[i])
                    {
                        case '"':
                            current.Append('"');
                            break;
                        case 'r':
                            current.Append('\r');
                            break;
                        case 'n':
                            current.Append('\n');
                            break;
                        default:
                            throw new Exception();
                    }
                }
                else
                    current.Append(argsPart[i]);
                backslash = false;
            }
            if (instring)
                throw new Exception();
            if (current.Length > 0)
            {
                result.Add(current.ToString());
            }
            return result;
        }
    }
}