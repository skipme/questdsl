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

        }
    }
}
