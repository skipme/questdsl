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
        public static void Check()
        {
            // warn: symlink never used
            // warn: var assigned but never been used
            // warn: invoke transition arguments misscount
            // warn: invoke transition argument missing
        }
    }
}
