using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace questdsl_tests
{
    [TestFixture]
    public class CodeCompletion
    {
        [Test]
        public void TestCompletion()
        {
            questdsl.State s1 = questdsl.Parser.ParseNode("somestate", "subval1\nsubval2\nsubval3");
            questdsl.State s2 = questdsl.Parser.ParseNode("sometrans", "--arg1 abc\n--arg2 def\n--arg3 xxx");
            questdsl.State s3 = questdsl.Parser.ParseNode("sometrig", "//\n");

            questdsl.Hinge h = new questdsl.Hinge(new List<questdsl.State>() { s1, s2, s3 });

            questdsl.Parser p = new questdsl.Parser();

            List<questdsl.Parser.SuggestIssue> issues = p.Suggest(h.GetStates(), h.GetTransitions(), "").ToList();
            Assert.AreEqual(issues.Count, 2);

            p.AppendLine("$x = 4442 + 1122");
            issues = p.Suggest(h.GetStates(), h.GetTransitions(), "").ToList();
            Assert.AreEqual(issues.Count, 2);

            p.AppendLine("\n");
            p.AppendLine("$x == 555");
            issues = p.Suggest(h.GetStates(), h.GetTransitions(), "").ToList();
            Assert.AreEqual(issues.Count, 2);
        }
    }
}