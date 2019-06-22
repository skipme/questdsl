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
                @"lidsted substates
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
                @"a: lidsted substates
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
        }
        [Test]
        public void TestTransition()
        {
        }
    }
}
