using System;
using System.Collections.Generic;
using System.Text;

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
            public List<Section> Sections;

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

                string keyName = substateName;

                if (keyName == null)
                {
                    if (SubStateMultiline == null)
                        throw new Exception();
                    keyName = SubStateMultiline;
                    SubStateMultiline = null;
                    if (Value == null)
                        throw new Exception();
                }
                if (Value != null)
                {
                    if (keyName == "$$$$$")
                    {
                        this.DeclareListedSubstate(Value);
                    }
                    else
                    {
                        StateNodeInstance.AddSubstate(keyName, Value);
                    }
                }
                else
                    SubStateMultiline = keyName;

                if (substateName != null)
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
                if (NodeDeclaredType == NodeType.State)
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
                if (NodeDeclaredType != NodeType.Transition)
                {
                    NodeDeclaredType = NodeType.Trigger;
                }
            }
            public void PushExec(ExpressionExecutive expression)
            {
                if (NodeDeclaredType == NodeType.State)
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

                if (NodeDeclaredType != NodeType.Transition)
                {
                    NodeDeclaredType = NodeType.Trigger;
                }
                this.SectionExecutionBody = true;
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
                if (this.NodeDeclaredType == NodeType.State)
                {
                    if (this.IsInMultivar)
                        this.AppendMultivar("");
                    return;
                }
                if (this.NodeDeclaredType == NodeType.undeclared)
                {
                    return;
                }
                if (!this.SectionExecutionBody && ProbesOr != null && ProbesOr.Count > 0)
                    throw new Exception();
                if (this.SectionExecutionBody && (ExecBody == null || ExecBody.Count == 0))
                    throw new Exception();

                if (!this.SectionExecutionBody)
                    return;

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
    }
}