using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace questdsl
{
    public partial class Parser
    {
        public class SuggestIssue
        {
            public string Name;
            public string Value;
        }
        readonly string[] controlSeq = new string[] { "----", "-->" };

        public IEnumerable<SuggestIssue> Suggest(IEnumerable<State> states, IEnumerable<Transition> transitions, string line)
        {
            // suggesting names:
            //                      localVar
            //                      state name
            //                      substate name (after (.))
            //                      transition name

            // structure control sequences:
            //                      ---- (after condition section part)
            //                      --> 

            string prefix = "";
            if (!string.IsNullOrWhiteSpace(line))
            {
                Match m = Regex.Match(line, @"^.*\s*(\w+)\s*$");
                if (m.Success)
                {
                    if (m.Groups[1].Success)
                    {
                        prefix = m.Groups[1].Value;
                    }
                }
            }
            //
            IEnumerable<SuggestIssue> varsc = from v in context.DefinedVars
                                              where v.StartsWith(prefix)
                                              select new SuggestIssue { Name = v + " (var)", Value = v };

            List<SuggestIssue> controls = new List<SuggestIssue>();
            if ((context.NodeDeclaredType == ParserContext.NodeType.undeclared ||
                context.NodeDeclaredType == ParserContext.NodeType.Transition)
                && !context.HasSections && (context.ExecBody == null || context.ExecBody.Count == 0)
                && (string.IsNullOrWhiteSpace(line) || "--arg".StartsWith(line.Trim())))
            {
                controls.Add(new SuggestIssue { Name = "--arg" + context.symlinks.Count + 1 + " varName", Value = "--arg" + context.symlinks.Count + 1 + " " });
            }
            if ((context.NodeDeclaredType == ParserContext.NodeType.undeclared || context.NodeDeclaredType == ParserContext.NodeType.Trigger ||
               context.NodeDeclaredType == ParserContext.NodeType.Transition)
               && (context.SectionExecutionBody || (context.NodeDeclaredType == ParserContext.NodeType.undeclared))
               && (string.IsNullOrWhiteSpace(line) || "--".StartsWith(line.Trim())))
            {
                foreach (var item in transitions)
                {
                    string args = "";
                    if (item.symlinks != null)
                        foreach (var a in item.symlinks)
                        {
                            args += " " + a.Value.VarName;
                        }
                    controls.Add(new SuggestIssue { Name = "-->" + item.Name, Value = "-->" + item.Name + args });
                }

            }
            else if ((context.NodeDeclaredType == ParserContext.NodeType.undeclared || context.NodeDeclaredType == ParserContext.NodeType.Trigger ||
              context.NodeDeclaredType == ParserContext.NodeType.Transition)
               && (context.SectionExecutionBody || (context.NodeDeclaredType == ParserContext.NodeType.undeclared))
              && line.Trim().StartsWith("-->"))
            {
                string name = line.Trim().Remove(0, 3);
                foreach (var item in transitions)
                {
                    if (!item.Name.StartsWith(name))
                        continue;

                    string args = "";
                    if (item.symlinks != null)
                        foreach (var a in item.symlinks)
                        {
                            args += " " + a.Value.VarName;
                        }
                    controls.Add(new SuggestIssue { Name = "-->" + item.Name, Value = "-->" + item.Name + args });
                }
            }

            if ((context.NodeDeclaredType == ParserContext.NodeType.Trigger ||
              context.NodeDeclaredType == ParserContext.NodeType.Transition)
                && !context.SectionExecutionBody && context.ProbesOr != null
               && "----".StartsWith(line))
                controls.Add(new SuggestIssue { Name = "----", Value = "----" });

            return varsc.Concat(controls);
        }
    }
}
