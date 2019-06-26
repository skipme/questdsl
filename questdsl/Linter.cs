using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    class Linter
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
            CheckSymlinks(h, issues);
            CheckImagesOps(h, issues);
            CheckVars(h, issues);
            CheckStates(h, issues);

            return issues;
        }
        private static void CheckVars(Hinge h, List<LintIssue> issues)
        {
            foreach (var t in h.Transitions.Concat(h.Triggers))
            {
                Dictionary<string, bool> usings = new Dictionary<string, bool>();
                if (!t.Value.IsTrigger)
                {
                    foreach (var s in t.Value.symlinks)
                    {
                        usings.Add(s.Value.VarName, false);
                    }
                }
                SortedSet<string> assignedVars = new SortedSet<string>();
                foreach (var sect in t.Value.sections)
                {
                    foreach (var ex in sect.Body)
                    {
                        foreach (var varname in ex.GetVarsUsed())
                        {
                            if (!assignedVars.Contains(varname) && usings.ContainsKey(varname))
                            {
                                issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "variable " + varname + " used before assignment" });
                            }
                        }
                        foreach (var varname in ex.GetVarsAssigned())
                        {
                            assignedVars.Add(varname);
                        }
                    }
                }
            }
        }
        private static void CheckStates(Hinge h, List<LintIssue> issues)
        {
            foreach (var t in h.Transitions.Concat(h.Triggers))
            {
                SortedSet<string> assignedVars = new SortedSet<string>();
                foreach (var sect in t.Value.sections)
                {
                    foreach (var ex in sect.Body)
                    {
                        Action<ExpressionValue> checkVal = (ExpressionValue ev) =>
                        {
                            if (ev != null
                                && ev.TypeOfReference == ExpressionValue.RefType.Substate
                                && ev.TypeValue == ExpressionValue.ValueType.SubstateName)
                            {
                                if (!h.States.ContainsKey(ev.Left))
                                {
                                    issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "substate " + ev.Left + " not found" });
                                }
                            }
                        };
                        checkVal(ex.AssignVar);
                        checkVal(ex.ExLeftPart);
                        checkVal(ex.ExRightPart);
                    }
                }
            }
        }
        private static void CheckNullOps(Hinge h, List<LintIssue> issues)
        {
        }
        private static void CheckImagesOps(Hinge h, List<LintIssue> issues)
        {
            foreach (var t in h.Transitions.Concat(h.Triggers))
            {
                Dictionary<string, bool> usings = new Dictionary<string, bool>();
                if (!t.Value.IsTrigger)
                {
                    foreach (var s in t.Value.symlinks)
                    {
                        usings.Add(s.Value.VarName, false);
                    }
                }

                foreach (var sect in t.Value.sections)
                {
                    foreach (var ex in sect.Body)
                    {
                        if (ex.AssignVar != null && (ex.AssignVar.TypeOfReference == ExpressionValue.RefType.Image
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
                        if (ex.AssignVar != null && ex.AssignVar.TypeOfReference == ExpressionValue.RefType.List)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.error, LineNumber = ex.LineNumber, Message = "list " + ex.AssignVar.ArgOrListIndex + " reassigned" });
                        }
                    }
                }
            }
        }
        private static void CheckSymlinks(Hinge h, List<LintIssue> issues)
        {
            foreach (var t in h.Transitions)
            {
                Dictionary<string, ExpressionSymlink> usingss = new Dictionary<string, ExpressionSymlink>();
                Dictionary<string, bool> usings = new Dictionary<string, bool>();
                foreach (var s in t.Value.symlinks)
                {
                    usings.Add(s.Value.VarName, false);
                    usingss.Add(s.Value.VarName, s.Value);
                }

                foreach (var sect in t.Value.sections)
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
                        if (ex.AssignVar != null && ex.AssignVar.TypeOfReference == ExpressionValue.RefType.Arg)
                        {
                            issues.Add(new LintIssue { IssueType = LintIssueType.warning, LineNumber = ex.LineNumber, Message = "arg " + ex.AssignVar.ArgOrListIndex + " reassigned" });
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
