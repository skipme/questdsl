using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            CheckNullOps(h, issues);
            CheckSymlinks(h.GetTransitions(), issues);
            CheckImagesOps(h.GetTransitions().Concat(h.GetTriggers()), issues);
            CheckVars(h.GetTransitions().Concat(h.GetTriggers()), issues);
            CheckStates(h.GetStates(), h.GetTransitions().Concat(h.GetTriggers()), issues);

            return issues;
        }
        public static List<LintIssue> CheckNode(Hinge h, State node)
        {
            List<LintIssue> issues = new List<LintIssue>();
            CheckNullOps(h, issues);

            if (node is Transition && !(node as Transition).IsTrigger)
                CheckSymlinks(new Transition[] { node as Transition }, issues);

            if (node is Transition)
                CheckImagesOps(new Transition[] { node as Transition }, issues);

            if (node is Transition)
                CheckVars(new Transition[] { node as Transition }, issues);

            if (node is Transition)
                CheckStates(h.GetStates(), new Transition[] { node as Transition }, issues);

            return issues;
        }
        private static void CheckVars(IEnumerable<Transition> transitionsAndTriggers, List<LintIssue> issues)
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
                    foreach (var ex in sect.Body)
                    {
                        foreach (var varname in ex.GetVarsReaded())
                        {
                            if (!assignedVars.Contains(varname) && !usings.ContainsKey(varname))
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
                    foreach (var ex in sect.Body)
                    {
                        Action<ExpressionValue> checkVal = (ExpressionValue ev) =>
                        {
                            if (ev != null
                                && ev.TypeOfReference == ExpressionValue.RefType.Substate
                                && ev.TypeOfValue == ExpressionValue.ValueType.SubstateName)
                            {
                                if (!(from s in states where s.Name == ev.Left select s).Any())
                                {
                                    issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "substate " + ev.Left + " not found" });
                                }
                            }
                        };
                        checkVal(ex.AssignResultVar);
                        checkVal(ex.ExLeftPart);
                        checkVal(ex.ExRightPart);
                    }
                }
            }
        }
        private static void CheckNullOps(Hinge h, List<LintIssue> issues)
        {
        }
        private static void CheckImagesOps(IEnumerable<Transition> transitionsAndTriggers, List<LintIssue> issues)
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
                    foreach (var ex in sect.Body)
                    {
                        foreach (var varu in ex.GetVarsUsed())
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
