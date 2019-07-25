using NUnit.Framework;
using System;

namespace questdsl_tests
{
    [TestFixture]
    public class ParseNodes
    {
        [Test]
        public void TestState()
        {
            questdsl.State stx = questdsl.Parser.ParseNode("xxx",
                @"listed substates
12223
""multiline text
ends here""

5512

11");
            Assert.AreEqual(stx.GetType(), typeof(questdsl.State));

            Assert.AreEqual(stx.Substates.Count, 5);
            Assert.AreEqual(stx.Substates[4].initialValue.Num, 11);
            stx = null;

            stx = questdsl.Parser.ParseNode("xxx",
                @"a: listed substates
b: 12223
c: ""multiline text
ends here""

d: 5512

x: ""other

multiline""

e: 11");
            Assert.AreEqual(stx.GetType(), typeof(questdsl.State));

            Assert.AreEqual(stx.Substates.Count, 6);
            Assert.AreEqual(stx.Substates[5].initialValue.Num, 11);
            Assert.AreEqual(stx.Substates[5].SubStateName, "e");
        }
        [Test]
        public void TestTrigger()
        {
            questdsl.State stx = questdsl.Parser.ParseNode("xxx",
@"
$assignment = uuo

depend.sub == text
----
$localvar += 1
depend.sub = $assignment
");
            Assert.AreEqual(stx.GetType(), typeof(questdsl.Transition));
            Assert.AreEqual((stx as questdsl.Transition).IsTrigger, true);
            Assert.AreEqual((stx as questdsl.Transition).sections.Count, 2);
            Assert.AreEqual((stx as questdsl.Transition).sections[0].ProbesOr, null);
            Assert.AreEqual((stx as questdsl.Transition).sections[0].Body.Count, 1);

            Assert.AreEqual((stx as questdsl.Transition).sections[1].ProbesOr.Count, 1);
            Assert.AreEqual((stx as questdsl.Transition).sections[1].Body.Count, 2);
        }
        [Test]
        public void TestTransition()
        {
            questdsl.State stx = questdsl.Parser.ParseNode("xxx",
@"trans
$assignment = uuo

depend.sub == text
----
$localvar += 1
depend.sub = $assignment
");
            Assert.AreEqual(stx.GetType(), typeof(questdsl.Transition));
            Assert.AreEqual((stx as questdsl.Transition).IsTrigger, false);

            stx = questdsl.Parser.ParseNode("xxx",
@"--arg4 $var0
$assignment = uuo

depend.sub == text
----
$localvar += 1
depend.sub = $assignment
");
            Assert.AreEqual(stx.GetType(), typeof(questdsl.Transition));
            Assert.AreEqual((stx as questdsl.Transition).IsTrigger, false);
            Assert.AreEqual((stx as questdsl.Transition).symlinks.Count, 1);
            Assert.AreEqual((stx as questdsl.Transition).symlinks[4].VarName, "var0");
        }
        [Test]
        public void TestDialog()
        {
            questdsl.State stx = questdsl.Parser.ParseNode("xxx",
                @"
>name text

>name2 text
>name2 text

>name text
");
            stx = questdsl.Parser.ParseNode("xxx",
                @"
x.y > 5
----
>name text

x.y < 5
----
>name2 text
>name2 text

>name text
state.val = 11
");

            Assert.AreEqual(stx.GetType(), typeof(questdsl.Dialogue));

            Assert.AreEqual(((questdsl.Dialogue)stx).sections.Count, 3);
            Assert.AreEqual(((questdsl.Dialogue)stx).sections[0].ProbesOr.Count, 1);
            Assert.AreEqual(((questdsl.Dialogue)stx).sections[0].ProbesOr[0].ExLeftPart.SubstatePath, "x.y");
            Assert.AreEqual(((questdsl.Dialogue)stx).sections[0].ProbesOr[0].ExRightPart.Num, 5);
            Assert.AreEqual(((questdsl.Dialogue)stx).sections[0].ProbesOr[0].ExOperation, questdsl.ExpressionBool.Operation.bt);
            Assert.AreEqual(((questdsl.Dialogue)stx).sections[2].Body[1].ExRightPart.Num, 11);
            stx = null;

            stx = questdsl.Parser.ParseNode("xxx",
                @">a listed substates
>b 12223
>c ""multiline text
ends here""

>d 5512

>x ""other

multiline""

>e 11");
            Assert.AreEqual(stx.GetType(), typeof(questdsl.Dialogue));

            Assert.AreEqual(((questdsl.Dialogue)stx).sections.Count, 4);
            Assert.AreEqual(((questdsl.Dialogue)stx).sections[2].Body[0].InvokeTransition, "say");
            Assert.AreEqual(((questdsl.Dialogue)stx).sections[2].Body[0].InvokeArgs[0].Left, "x");
            Assert.AreEqual(((questdsl.Dialogue)stx).sections[3].Body[0].InvokeArgs[0].Left, "e");
            Assert.AreEqual(((questdsl.Dialogue)stx).sections[3].Body[0].InvokeArgs[1].Num, 11);
        }
    }
}
