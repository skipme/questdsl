using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace questdsl_tests
{
    [TestFixture]
    public class LineTypes
    {
        [Test]
        public void TestPartTypes()
        {
            questdsl.Parser parser = new questdsl.Parser();
            Assert.AreEqual(parser.EvaluatePartType("a.b"), questdsl.Parser.PartType.substate);
            Assert.AreEqual(parser.EvaluatePartType("a.$b"), questdsl.Parser.PartType.substate_subVar);
            Assert.AreEqual(parser.EvaluatePartType("$a.b"), questdsl.Parser.PartType.substate_stateVar);
            Assert.AreEqual(parser.EvaluatePartType("$a.$b"), questdsl.Parser.PartType.substate_allVar);

            Assert.AreEqual(parser.EvaluatePartType("\"string\""), questdsl.Parser.PartType.text_multiline);
            Assert.AreEqual(parser.EvaluatePartType("\"stri\"ng\""), questdsl.Parser.PartType.text_multiline);
            Assert.AreEqual(parser.EvaluatePartType("\"string xxx"), questdsl.Parser.PartType.text_multiline_start);
            Assert.AreEqual(parser.EvaluatePartType("3xxx string\""), questdsl.Parser.PartType.text_multiline_end);
            Assert.AreEqual(parser.EvaluatePartType("234235346546"), questdsl.Parser.PartType.digit);

            Dictionary<string, string> groups = new Dictionary<string, string>();
            parser.EvaluatePartType("intermediate\"", groups);
            Assert.AreEqual(groups["string"], "intermediate");
            groups.Clear();
            parser.EvaluatePartType("\"intermediate", groups);
            Assert.AreEqual(groups["string"], "intermediate");
        }
        [Test]
        public void TestLineTypes()
        {
            questdsl.Parser parser = new questdsl.Parser();
            Assert.AreEqual(parser.EvaluateLineType("\\commentary"), questdsl.Parser.LineType.comment);
            Assert.AreEqual(parser.EvaluateLineType("\\"), questdsl.Parser.LineType.comment);
            Assert.AreEqual(parser.EvaluateLineType(""), questdsl.Parser.LineType.empty);
            Assert.AreEqual(parser.EvaluateLineType(" "), questdsl.Parser.LineType.empty);

            Assert.AreEqual(parser.EvaluateLineType("----"), questdsl.Parser.LineType.section_separator);
            Assert.AreEqual(parser.EvaluateLineType("---"), questdsl.Parser.LineType.section_separator);
            Assert.AreEqual(parser.EvaluateLineType("----------"), questdsl.Parser.LineType.section_separator);
            Assert.AreEqual(parser.EvaluateLineType("substate: val"), questdsl.Parser.LineType.substate_declaration);
            Assert.AreEqual(parser.EvaluateLineType("$substate: val"), questdsl.Parser.LineType.substate_declaration);
            Assert.AreEqual(parser.EvaluateLineType("substate.x: val"), questdsl.Parser.LineType.substate_declaration);
            Assert.AreEqual(parser.EvaluateLineType("substate: $val"), questdsl.Parser.LineType.substate_declaration);
            Assert.AreEqual(parser.EvaluateLineType("substate.$: val"), questdsl.Parser.LineType.substate_declaration);

            Assert.AreEqual(parser.EvaluateLineType("--arg9 mnemo"), questdsl.Parser.LineType.simlink);
            Assert.AreEqual(parser.EvaluateLineType(" "), questdsl.Parser.LineType.empty);



            Assert.AreEqual(parser.EvaluateLineType("a>b"), questdsl.Parser.LineType.condition);
            Assert.AreEqual(parser.EvaluateLineType("a<b"), questdsl.Parser.LineType.condition);
            Assert.AreEqual(parser.EvaluateLineType("a==b"), questdsl.Parser.LineType.condition);
            Assert.AreEqual(parser.EvaluateLineType("a!=b"), questdsl.Parser.LineType.condition);
            Assert.AreEqual(parser.EvaluateLineType("a>=b"), questdsl.Parser.LineType.condition);
            Assert.AreEqual(parser.EvaluateLineType("a<=b"), questdsl.Parser.LineType.condition);
            Assert.AreEqual(parser.EvaluateLineType("a=>b"), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("a=<b"), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("<"), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("<<"), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType(">"), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("!x"), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("a!b"), questdsl.Parser.LineType.undetermined);

            Assert.AreEqual(parser.EvaluateLineType("a=b"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("a++"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("a+=b"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("a--"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("a-=b"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("a=b"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("a=$b"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("a=$b.b"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("a=b.$b"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("a=$b.$b"), questdsl.Parser.LineType.executive);
            Assert.AreEqual(parser.EvaluateLineType("$a =string value"), questdsl.Parser.LineType.executive);

            Assert.AreEqual(parser.EvaluateLineType("-->a"), questdsl.Parser.LineType.executive_invocation);
            Assert.AreEqual(parser.EvaluateLineType("-->a c d e"), questdsl.Parser.LineType.executive_invocation);
            Assert.AreEqual(parser.EvaluateLineType("-->a c d $e"), questdsl.Parser.LineType.executive_invocation);
            Assert.AreEqual(parser.EvaluateLineType("-->a c d $e.e"), questdsl.Parser.LineType.executive_invocation);
            Assert.AreEqual(parser.EvaluateLineType("-->a c d $e.$e"), questdsl.Parser.LineType.executive_invocation);
            Assert.AreEqual(parser.EvaluateLineType("-->a c d e.$e"), questdsl.Parser.LineType.executive_invocation);
            Assert.AreEqual(parser.EvaluateLineType("="), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("a="), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("=a"), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("a===a"), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("===a"), questdsl.Parser.LineType.undetermined);
            Assert.AreEqual(parser.EvaluateLineType("a==="), questdsl.Parser.LineType.undetermined);

        }
    }
}
