using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl_tests
{
    [TestFixture]
    public class ParserSection
    {
        [Test]
        public void TestSubstates()
        {
            questdsl.Parser p = new questdsl.Parser();
            p.AppendLine("name: text");
            Assert.IsTrue(p.context.DeclaredSubstatesByName);
            Assert.IsFalse(p.context.DeclaredSubstatesByList);
            Assert.AreEqual(p.context.IsInMultivar, false);
            Assert.AreEqual(p.context.NodeDeclaredType, questdsl.Parser.ParserContext.NodeType.State);
            Assert.AreEqual(p.context.StateNodeInstance.Substates.Count, 1);
            Assert.AreEqual(p.context.StateNodeInstance.SubstatesBook["name"].initialValue.TypeValue, questdsl.ExpressionValue.ValueType.string_text);
            Assert.IsTrue(string.CompareOrdinal(p.context.StateNodeInstance.SubstatesBook["name"].initialValue.Left, "text") == 0);

            Assert.Throws<Exception>(() => p.AppendLine("sometext"));

            p = new questdsl.Parser();
            p.AppendLine("someLine");
            Assert.IsFalse(p.context.DeclaredSubstatesByName);
            Assert.IsTrue(p.context.DeclaredSubstatesByList);
            Assert.AreEqual(p.context.IsInMultivar, false);
            Assert.AreEqual(p.context.NodeDeclaredType, questdsl.Parser.ParserContext.NodeType.State);
            Assert.AreEqual(p.context.StateNodeInstance.Substates.Count, 1);
            Assert.AreEqual(p.context.StateNodeInstance.Substates[0].initialValue.TypeValue, questdsl.ExpressionValue.ValueType.string_text);
            Assert.IsTrue(string.CompareOrdinal(p.context.StateNodeInstance.Substates[0].initialValue.Left, "someLine") == 0);

            Assert.Throws<Exception>(() => p.AppendLine("name: sometext"));

            p.AppendLine("9995");
            Assert.IsFalse(p.context.DeclaredSubstatesByName);
            Assert.IsTrue(p.context.DeclaredSubstatesByList);
            Assert.AreEqual(p.context.IsInMultivar, false);
            Assert.AreEqual(p.context.NodeDeclaredType, questdsl.Parser.ParserContext.NodeType.State);
            Assert.AreEqual(p.context.StateNodeInstance.Substates.Count, 2);
            Assert.AreEqual(p.context.StateNodeInstance.Substates[1].initialValue.TypeValue, questdsl.ExpressionValue.ValueType.number);
            Assert.IsTrue(p.context.StateNodeInstance.Substates[1].initialValue.Num == 9995);

            p.AppendLine("\"multilineStart");
            Assert.AreEqual(p.context.IsInMultivar, true);
            Assert.AreEqual(p.context.StateNodeInstance.Substates.Count, 2);
            p.AppendLine("intermediate\"xxx");
            Assert.AreEqual(p.context.IsInMultivar, true);
            Assert.AreEqual(p.context.StateNodeInstance.Substates.Count, 2);
            p.AppendLine("intermediate\"");
            Assert.AreEqual(p.context.IsInMultivar, false);
            Assert.AreEqual(p.context.StateNodeInstance.Substates.Count, 3);

            Assert.IsTrue(string.CompareOrdinal(p.context.StateNodeInstance.Substates[2].initialValue.Left, "multilineStart\r\nintermediate\"xxx\r\nintermediate") == 0);



        }
        [Test]
        public void TestSimlink()
        {
            questdsl.Parser p = new questdsl.Parser();
            p.AppendLine("--arg1 parameter");
            Assert.AreEqual(p.context.simlinks.Count, 1);
            Assert.AreEqual(p.context.simlinks[1], "parameter");

            p.AppendLine("--arg2 $parameternew");
            Assert.AreEqual(p.context.simlinks.Count, 2);
            Assert.AreEqual(p.context.simlinks[2], "parameternew");

            Assert.Throws<Exception>(() => p.AppendLine("--arg1 parameterb"));
            Assert.Throws<Exception>(() => p.AppendLine("--arg0 parameterb"));
            Assert.Throws<Exception>(() => p.AppendLine("--arg3 parameter"));
        }
    }
}
