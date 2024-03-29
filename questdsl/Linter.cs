﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace questdsl
{
    public class Linter
    {
        public enum LintIssueType
        {
            warning,
            error
        }
        public class LintIssue
        {
            public LintIssueType IssueType;
            public string Message;
            public int LineNumber;
        }
        public static List<LintIssue> Check(Hinge h)
        {
            // warn: symlink never been used
            // warn: symlink reassigned
            // warn: var assigned but never been used
            // warn: invoke transition arguments misscount
            // warn: invoke transition argument missing

            // warn: substate not found

            // err: state not found
            // err: local var used before assignment
            List<LintIssue> issues = new List<LintIssue>();
            CheckNullOps(h, h.GetTransitions().Concat(h.GetTriggers()), issues);
            CheckSymlinks(h.GetTransitions(), issues);
            CheckImagesOps(h.GetTransitions().Concat(h.GetTriggers()), issues);
            CheckVars(h, h.GetTransitions().Concat(h.GetTriggers()), issues, h.InteropNames);
            CheckStates(h.GetStates(), h.GetTransitions().Concat(h.GetTriggers()), issues);

            return issues;
        }
        public static List<LintIssue> CheckNode(Hinge h, State node)
        {
            List<LintIssue> issues = new List<LintIssue>();
            CheckNullOps(h, h.GetTransitions().Concat(h.GetTriggers()), issues);

            if (node is Transition && !(node as Transition).IsTrigger)
                CheckSymlinks(new Transition[] { node as Transition }, issues);

            if (node is Transition)
                CheckImagesOps(new Transition[] { node as Transition }, issues);

            if (node is Transition)
                CheckVars(h, new Transition[] { node as Transition }, issues, h.InteropNames);

            if (node is Transition)
                CheckStates(h.GetStates(), new Transition[] { node as Transition }, issues);

            return issues;
        }
        private static void CheckVars(Hinge h, IEnumerable<Transition> transitionsAndTriggers, List<LintIssue> issues, SortedSet<string> invocationNames)
        {
            foreach (var t in transitionsAndTriggers)
            {
                Dictionary<string, bool> usings = new Dictionary<string, bool>();
                if (!t.IsTrigger)
                {
                    foreach (var s in t.symlinks)
                    {
                        usings.Add(s.Value.VarName, false);
                    }
                }
                SortedSet<string> assignedVars = new SortedSet<string>();
                foreach (var sect in t.sections)
                {
                    bool listDefined = false;
                    State toListNode = null;
                    foreach (var ex in sect.Body)
                    {

                        if (ex.FuncType == ExpressionExecutive.ExecuteType.Invocation
                            && ((!h.AllNodesDict.ContainsKey(ex.InvokeTransitionName))
                            || (!(h.AllNodesDict[ex.InvokeTransitionName] is Transition) && (!(h.AllNodesDict[ex.InvokeTransitionName] is Dialogue)))
                            || (h.AllNodesDict[ex.InvokeTransitionName] is Transition) && (h.AllNodesDict[ex.InvokeTransitionName] as Transition).IsTrigger))
                        {
                            if (ex.InvokeTransitionName == "say" && !(t is Dialogue))
                            {
                                issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = $"say proc allowed only in dialogues " });
                            }
                            else
                            if (!invocationNames.Contains(ex.InvokeTransitionName))
                            {
                                issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = $"Invocation \"{ex.InvokeTransitionName}\" not found, or not transition or dialogue " });
                            }
                        }
                        if (ex.FuncType == ExpressionExecutive.ExecuteType.ToList)
                        {
                            if (!h.AllNodesDict.ContainsKey(ex.InvokeArgs[0].Left))
                            {
                                issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = $"State node \"{ex.InvokeArgs[0].Left}\" not found, toList op " });
                            }
                            else
                            {
                                toListNode = h.AllNodesDict[ex.InvokeArgs[0].Left];
                                listDefined = true;
                            }
                        }
                        if ((ex.ExLeftPart != null && ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.List)
                            || (ex.ExRightPart != null && ex.ExRightPart.TypeOfReference == ExpressionValue.RefType.List))
                        {
                            if (!listDefined)
                            {
                                issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = $"list reference used before assignment" });
                            }
                            else
                            {
                                if (ex.ExLeftPart != null && ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.List
                                    && (toListNode.Substates.Count <= ex.ExLeftPart.ArgOrListIndex || ex.ExLeftPart.ArgOrListIndex < 0))
                                {
                                    issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = $"list reference {ex.ExLeftPart.ArgOrListIndex} out of substate list bounds" });
                                }
                                if (ex.ExRightPart != null && ex.ExRightPart.TypeOfReference == ExpressionValue.RefType.List
                                    && (toListNode.Substates.Count <= ex.ExRightPart.ArgOrListIndex || ex.ExRightPart.ArgOrListIndex < 0))
                                {
                                    issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = $"list reference {ex.ExRightPart.ArgOrListIndex} out of substate list bounds" });
                                }
                            }
                        }

                        foreach (var varname in ex.GetVarsReaded())
                        {
                            if (!assignedVars.Contains(varname) && !usings.ContainsKey(varname) && varname != "image")
                            {
                                issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "variable " + varname + " used before assignment" });
                            }
                        }
                        foreach (var varas in ex.GetVarsAssigned())
                        {
                            assignedVars.Add(varas);
                        }
                    }
                }
            }
        }
        private static void CheckStates(IEnumerable<State> states, IEnumerable<Transition> transitionsAndTriggers, List<LintIssue> issues)
        {
            foreach (var t in transitionsAndTriggers)
            {
                SortedSet<string> assignedVars = new SortedSet<string>();
                foreach (var sect in t.sections)
                {
                    Action<ExpressionValue, int> checkVal = (ExpressionValue ev, int linen) =>
                    {
                        if (ev != null
                            && ev.TypeOfReference == ExpressionValue.RefType.Substate
                            && (ev.TypeOfValue == ExpressionValue.ValueType.SubstateName || ev.TypeOfValue == ExpressionValue.ValueType.StateName_SubstateRef))
                        {
                            if (!(from s in states where s.Name == ev.Left select s).Any())
                            {
                                issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = linen, Message = "state " + ev.Left + " not found" });
                            }
                            else if (ev.TypeOfValue == ExpressionValue.ValueType.SubstateName
                            && !((from s in states where s.Name == ev.Left select s).First().SubstatesBook.ContainsKey(ev.Right)))
                            {
                                issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = linen, Message = $"substate `{ev.Right}` in `{ev.Left}` not found" });
                            }
                        }
                    };
                    if (sect.ProbesOr != null)
                    {
                        foreach (var p in sect.ProbesOr)
                        {
                            checkVal(p.ExLeftPart, p.LineNumber);
                            checkVal(p.ExRightPart, p.LineNumber);
                        }
                    }
                    foreach (var ex in sect.Body)
                    {
                        checkVal(ex.AssignResultVar, ex.LineNumber);
                        checkVal(ex.ExLeftPart, ex.LineNumber);
                        checkVal(ex.ExRightPart, ex.LineNumber);

                        if (ex.InvokeArgs != null)
                        {
                            if (ex.InvokeTransitionName != "say" && t is Dialogue)
                            {
                                foreach (var item in ex.InvokeArgs)
                                {
                                    checkVal(item, ex.LineNumber);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void CheckNullOps(Hinge h, IEnumerable<Transition> transitionsAndTriggers, List<LintIssue> issues)
        {
            foreach (var t in transitionsAndTriggers)
            {
                foreach (var sect in t.sections)
                {
                    foreach (var ex in sect.Body)
                    {
                        if (ex.AssignResultVar != null && (ex.AssignResultVar.TypeOfReference == ExpressionValue.RefType.Null
                            || ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.Null
                            || ex.ExRightPart.TypeOfReference == ExpressionValue.RefType.Null))
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "null ref not allowed in this assign operation" });
                        }
                        else
                        if (ex.ExLeftPart != null && ex.FuncType == ExpressionExecutive.ExecuteType.Assign
                            && ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.Null &&
                            ex.ExRightPart != null && ex.ExRightPart.TypeOfReference != ExpressionValue.RefType.Null)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "null ref not allowed for non image assign" });
                        }
                        else
                        if (ex.ExLeftPart != null && ex.FuncType != ExpressionExecutive.ExecuteType.Assign
                            && ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.Null)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "null ref not allowed in this op" });
                        }
                    }
                }
            }
        }
        private static void CheckImagesOps(IEnumerable<Transition> transitionsAndTriggers, List<LintIssue> issues)
        {
            foreach (var t in transitionsAndTriggers)
            {
                foreach (var sect in t.sections)
                {
                    foreach (var ex in sect.Body)
                    {
                        if (ex.AssignResultVar != null && (ex.AssignResultVar.TypeOfReference == ExpressionValue.RefType.Image
                            || ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.Image
                            || ex.ExRightPart.TypeOfReference == ExpressionValue.RefType.Image))
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "image ref not allowed in this assign operation" });
                        }
                        else
                        if (ex.ExLeftPart != null && ex.FuncType == ExpressionExecutive.ExecuteType.Assign
                            && ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.Image &&
                            ex.ExRightPart != null && ex.ExRightPart.TypeOfReference != ExpressionValue.RefType.Image)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "image ref not allowed for non image assign" });
                        }
                        else
                        if (ex.ExLeftPart != null && ex.FuncType != ExpressionExecutive.ExecuteType.Assign
                            && ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.Image)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "image ref not allowed in this op" });
                        }

                        if (ex.ExLeftPart != null && ex.FuncType == ExpressionExecutive.ExecuteType.Assign && ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.List)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "list " + ex.ExLeftPart.ArgOrListIndex + " reassigned" });
                        }
                        if (ex.AssignResultVar != null && ex.AssignResultVar.TypeOfReference == ExpressionValue.RefType.List)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "list " + ex.AssignResultVar.ArgOrListIndex + " reassigned" });
                        }
                    }
                }
            }
        }
        private static void CheckSymlinks(IEnumerable<Transition> transitions, List<LintIssue> issues)
        {
            foreach (var t in transitions)
            {
                Dictionary<string, ExpressionSymlink> usingss = new Dictionary<string, ExpressionSymlink>();
                Dictionary<string, bool> usings = new Dictionary<string, bool>();
                foreach (var s in t.symlinks)
                {
                    usings.Add(s.Value.VarName, false);
                    usingss.Add(s.Value.VarName, s.Value);
                }

                foreach (var sect in t.sections)
                {
                    if (sect.ProbesOr != null)
                    {
                        foreach (var p in sect.ProbesOr)
                        {
                            foreach (var varu in p.GetVarsInScope())
                            {
                                if (usings.ContainsKey(varu))
                                    usings[varu] = true;
                            }
                        }
                    }
                    foreach (var ex in sect.Body)
                    {
                        foreach (var varu in ex.GetVarsInScope())
                        {
                            if (usings.ContainsKey(varu))
                                usings[varu] = true;
                        }
                        foreach (var vara in ex.GetVarsAssigned())
                        {
                            if (usings.ContainsKey(vara))
                            {
                                issues.Add(new LintIssue { IssueType = LintIssueType.warning, LineNumber = ex.LineNumber, Message = "symlink arg " + vara + " reassigned" });
                            }
                        }
                        if (ex.AssignResultVar != null && ex.AssignResultVar.TypeOfReference == ExpressionValue.RefType.Arg)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.warning, LineNumber = ex.LineNumber, Message = "arg " + ex.AssignResultVar.ArgOrListIndex + " reassigned" });
                        }
                        if (ex.ExLeftPart != null && ex.FuncType == ExpressionExecutive.ExecuteType.Assign && ex.ExLeftPart.TypeOfReference == ExpressionValue.RefType.Arg)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.warning, LineNumber = ex.LineNumber, Message = "arg " + ex.ExLeftPart.ArgOrListIndex + " reassigned" });
                        }
                    }
                }
                foreach (var s in usings)
                {
                    if (!s.Value)
                    {
                        issues.Add(new LintIssue { IssueType = LintIssueType.warning, LineNumber = usingss[s.Key].LineNumber, Message = "symlink arg " + s.Key + " not used anywhere" });
                    }
                }
            }
        }
    }
}
