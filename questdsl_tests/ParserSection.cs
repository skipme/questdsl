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
