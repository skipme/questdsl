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
        public void TestExecExpressions()
        {
            questdsl.Parser p = new questdsl.Parser();
            Assert.Throws<Exception>(() => p.AppendLine("a=$c"));

            p.AppendLine("$a= that is it ");
            Assert.AreEqual(p.context.ExecBody.Count, 1);
            Assert.AreEqual(p.context.ExecBody[0].ExLeftPart.TypeOfReference, questdsl.ExpressionValue.RefType.LocalVar);
            Assert.AreEqual(p.context.ExecBody[0].ExRightPart.TypeValue, questdsl.ExpressionValue.ValueType.string_text);
            Assert.AreEqual(p.context.ExecBody[0].ExRightPart.Left, "that is it");

            Assert.Throws<Exception>(() => p.AppendLine("$a==$x"));
            p.AppendLine("");
            Assert.DoesNotThrow(() => p.AppendLine("$a==$x"));
            Assert.Throws<Exception>(() => p.AppendLine(""));
            Assert.Throws<Exception>(() => p.AppendLine("$m+=1"));
            Assert.DoesNotThrow(() => p.AppendLine("----"));
            Assert.Throws<Exception>(() => p.AppendLine("----"));
            Assert.DoesNotThrow(() => p.AppendLine("$m+=1"));

            p = new questdsl.Parser();
            Assert.Throws<Exception>(() => p.AppendLine("$v = \"xxxx "));
            Assert.Throws<Exception>(() => p.AppendLine("$v = xxxx\" "));
            Assert.DoesNotThrow(() => p.AppendLine("$v = xxxx\"yyy "));
            Assert.DoesNotThrow(() => p.AppendLine("$v = \"xxxxyyy\" "));

            p = new questdsl.Parser();
            p.AppendLine("trans");
            Assert.AreEqual(p.context.NodeDeclaredType, questdsl.Parser.ParserContext.NodeType.Transition);

            p = new questdsl.Parser();
            p.AppendLine("-->move location");
            Assert.AreEqual(p.context.ExecBody.Count, 1);
            Assert.AreEqual(p.context.ExecBody[0].InvokeTransition, "move");
            Assert.AreEqual(p.context.ExecBody[0].FuncType, questdsl.ExpressionExecutive.ExecuteType.Invocation);
            Assert.AreEqual(p.context.ExecBody[0].InvokeArgs.Count, 1);
            Assert.AreEqual(p.context.ExecBody[0].InvokeArgs[0].TypeValue, questdsl.ExpressionValue.ValueType.string_text);
            Assert.AreEqual(p.context.ExecBody[0].InvokeArgs[0].Left, "location");

            p = new questdsl.Parser();
            p.AppendLine("$var = $k + 1");
            Assert.AreEqual(p.context.ExecBody.Count, 1);
            Assert.AreEqual(p.context.ExecBody[0].AssignVar.Left, "var");
            Assert.AreEqual(p.context.ExecBody[0].AssignVar.TypeOfReference, questdsl.ExpressionValue.RefType.LocalVar);
            Assert.AreEqual(p.context.ExecBody[0].FuncType, questdsl.ExpressionExecutive.ExecuteType.AssignAdd);
            Assert.AreEqual(p.context.ExecBody[0].ExLeftPart.Left, "k");
            Assert.AreEqual(p.context.ExecBody[0].ExLeftPart.TypeOfReference, questdsl.ExpressionValue.RefType.LocalVar);
            Assert.AreEqual(p.context.ExecBody[0].ExRightPart.Num, 1);
        }

        [Test]
        public void TestBoolExpressions()
        {
            questdsl.Parser p = new questdsl.Parser();
            p.AppendLine("a!=$c");
            Assert.AreEqual(p.context.ProbesOr.Count, 1);
            Assert.AreEqual(p.context.ProbesOr[0].ExRightPart.TypeOfReference, questdsl.ExpressionValue.RefType.LocalVar);
            Assert.AreEqual(p.context.ProbesOr[0].ExLeftPart.TypeValue, questdsl.ExpressionValue.ValueType.string_text);

            p = new questdsl.Parser();
            p.AppendLine("a string on left side !=$c");
            Assert.AreEqual(p.context.ProbesOr.Count, 1);
            Assert.AreEqual(p.context.ProbesOr[0].ExRightPart.TypeOfReference, questdsl.ExpressionValue.RefType.LocalVar);
            Assert.AreEqual(p.context.ProbesOr[0].ExLeftPart.TypeValue, questdsl.ExpressionValue.ValueType.string_text);
            Assert.AreEqual(p.context.ProbesOr[0].ExLeftPart.Left, "a string on left side");

            p = new questdsl.Parser();
            p.AppendLine("\"a string on left side\" !=$c");
            Assert.AreEqual(p.context.ProbesOr.Count, 1);
            Assert.AreEqual(p.context.ProbesOr[0].ExRightPart.TypeOfReference, questdsl.ExpressionValue.RefType.LocalVar);
            Assert.AreEqual(p.context.ProbesOr[0].ExLeftPart.TypeValue, questdsl.ExpressionValue.ValueType.string_text);
            Assert.AreEqual(p.context.ProbesOr[0].ExLeftPart.Left, "a string on left side");

            Assert.Throws<Exception>(() => p.AppendLine("a!=c"));

            p = new questdsl.Parser();
            p.AppendLine("$c == 41");
            Assert.AreEqual(p.context.ProbesOr.Count, 1);
            Assert.AreEqual(p.context.ProbesOr[0].ExOperation, questdsl.ExpressionBool.Operation.eq);
            Assert.AreEqual(p.context.ProbesOr[0].ExLeftPart.TypeOfReference, questdsl.ExpressionValue.RefType.LocalVar);
            Assert.AreEqual(p.context.ProbesOr[0].ExRightPart.TypeValue, questdsl.ExpressionValue.ValueType.number);
            Assert.AreEqual(p.context.ProbesOr[0].ExRightPart.Num, 41);

            p = new questdsl.Parser();
            p.AppendLine("$c != $param");
            Assert.AreEqual(p.context.ProbesOr.Count, 1);
            Assert.AreEqual(p.context.ProbesOr[0].ExOperation, questdsl.ExpressionBool.Operation.neq);

            p = new questdsl.Parser();
            p.AppendLine("$c > $param");
            Assert.AreEqual(p.context.ProbesOr.Count, 1);
            Assert.AreEqual(p.context.ProbesOr[0].ExOperation, questdsl.ExpressionBool.Operation.bt);

            p = new questdsl.Parser();
            p.AppendLine("$c >= $param");
            Assert.AreEqual(p.context.ProbesOr.Count, 1);
            Assert.AreEqual(p.context.ProbesOr[0].ExOperation, questdsl.ExpressionBool.Operation.bteq);


            p = new questdsl.Parser();
            p.AppendLine("$c < $param");
            Assert.AreEqual(p.context.ProbesOr.Count, 1);
            Assert.AreEqual(p.context.ProbesOr[0].ExOperation, questdsl.ExpressionBool.Operation.lt);


            p = new questdsl.Parser();
            p.AppendLine("$c <= $param");
            Assert.AreEqual(p.context.ProbesOr.Count, 1);
            Assert.AreEqual(p.context.ProbesOr[0].ExOperation, questdsl.ExpressionBool.Operation.lteq);

            Assert.Throws<Exception>(() => p.AppendLine("$a=$x"));
            Assert.Throws<Exception>(() => p.AppendLine(""));
            p.AppendLine("----");
            Assert.DoesNotThrow(() => p.AppendLine("$a=$x"));

        }
    }
}
