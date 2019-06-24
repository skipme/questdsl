using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl_tests
{
    [TestFixture]
    public class ParseValues
    {
        [Test]
        public void TestValExpressions()
        {
            questdsl.Parser p = new questdsl.Parser();
            questdsl.ExpressionValue val = p.ParseValue("abra");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.NotReferred);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.string_text);
            Assert.AreEqual(val.Left, "abra");
            val = p.ParseValue("abra cadabbra");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.NotReferred);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.string_text);
            Assert.AreEqual(val.Left, "abra cadabbra");

            val = p.ParseValue(" \"avvada ceddavra\" ");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.NotReferred);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.string_text);
            Assert.AreEqual(val.Left, "avvada ceddavra");

            val = p.ParseValue("4657");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.NotReferred);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.number);
            Assert.AreEqual(val.Left, null);
            Assert.AreEqual(val.Num, 4657);

            val = p.ParseValue("$variable");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.LocalVar);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.Reference);
            Assert.AreEqual(val.Left, "variable");
            Assert.AreEqual(val.vars.Count, 1);
            Assert.AreEqual(val.vars[0], "variable");

            Assert.Throws<Exception>(() => p.ParseValue("$variable with spaces"));

            val = p.ParseValue("$null");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.Null);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.Reference);
            Assert.AreEqual(val.Left, "null");
            Assert.AreEqual(val.vars.Count, 1);
            Assert.AreEqual(val.vars[0], "null");

            val = p.ParseValue(" null");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.Null);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.Reference);
            Assert.AreEqual(val.Left, "null");
            Assert.AreEqual(val.vars.Count, 1);
            Assert.AreEqual(val.vars[0], "null");

            val = p.ParseValue("$arg4");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.Arg);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.Reference);
            Assert.AreEqual(val.ArgOrListIndex, 4);
            Assert.AreEqual(val.Left, "arg4");
            Assert.AreEqual(val.vars.Count, 1);
            Assert.AreEqual(val.vars[0], "arg4");

            val = p.ParseValue("$list14");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.List);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.Reference);
            Assert.AreEqual(val.ArgOrListIndex, 14);
            Assert.AreEqual(val.Left, "list14");
            Assert.AreEqual(val.vars.Count, 1);
            Assert.AreEqual(val.vars[0], "list14");

            val = p.ParseValue("$image");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.Image);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.Reference);
            Assert.AreEqual(val.Left, "image");
            Assert.AreEqual(val.vars.Count, 1);
            Assert.AreEqual(val.vars[0], "image");

            val = p.ParseValue(" statename.subname ");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.Substate);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.SubstateName);
            Assert.AreEqual(val.Left, "statename");
            Assert.AreEqual(val.Right, "subname");
            Assert.AreEqual(val.vars.Count, 0);

            val = p.ParseValue(" statename.$subname ");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.Substate);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.StateName_SubstateRef);
            Assert.AreEqual(val.Left, "statename");
            Assert.AreEqual(val.Right, "subname");
            Assert.AreEqual(val.vars.Count, 1);
            Assert.AreEqual(val.vars[0], "subname");

            val = p.ParseValue(" $statename.subname ");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.Substate);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.StateRef_SubstateName);
            Assert.AreEqual(val.Left, "statename");
            Assert.AreEqual(val.Right, "subname");
            Assert.AreEqual(val.vars.Count, 1);
            Assert.AreEqual(val.vars[0], "statename");

            val = p.ParseValue(" $statename.$subname ");
            Assert.AreEqual(val.TypeOfReference, questdsl.ExpressionValue.RefType.Substate);
            Assert.AreEqual(val.TypeValue, questdsl.ExpressionValue.ValueType.StateRef_SubstateRef);
            Assert.AreEqual(val.Left, "statename");
            Assert.AreEqual(val.Right, "subname");
            Assert.AreEqual(val.vars.Count, 2);
            Assert.AreEqual(val.vars[0], "statename");
            Assert.AreEqual(val.vars[1], "subname");
        }
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
            Assert.AreEqual(p.context.symlinks.Count, 1);
            Assert.AreEqual(p.context.symlinks[1], "parameter");

            p.AppendLine("--arg2 $parameternew");
            Assert.AreEqual(p.context.symlinks.Count, 2);
            Assert.AreEqual(p.context.symlinks[2], "parameternew");

            Assert.Throws<Exception>(() => p.AppendLine("--arg1 parameterb"));
            Assert.Throws<Exception>(() => p.AppendLine("--arg0 parameterb"));
            Assert.Throws<Exception>(() => p.AppendLine("--arg3 parameter"));
        }
    }
}
