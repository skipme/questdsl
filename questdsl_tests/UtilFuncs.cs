using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace questdsl_tests
{
    [TestFixture]
    public class UtilFuncs
    {
        [Test]
        public void TestParserUtils()
        {
            questdsl.Parser parser = new questdsl.Parser();
            List<string> parts = parser.SplitArgs("xxx xxx.yyy \"with spaces\" 333");
            Assert.AreEqual(parts.Count, 4);
            Assert.AreEqual(parts[0], "xxx");
            Assert.AreEqual(parts[1], "xxx.yyy");
            Assert.AreEqual(parts[2], "with spaces");
            Assert.AreEqual(parts[3], "333");

            parts = parser.SplitArgs("xxx xxx.yyy \"with \\\" spaces\" 333");
            Assert.AreEqual(parts.Count, 4);
            Assert.AreEqual(parts[0], "xxx");
            Assert.AreEqual(parts[1], "xxx.yyy");
            Assert.AreEqual(parts[2], "with \" spaces");
            Assert.AreEqual(parts[3], "333");

            parts = parser.SplitArgs(" rrrr");
            Assert.AreEqual(parts[0], "rrrr");

            Assert.Throws<Exception>(() => parser.SplitArgs("xxx xxx.yyy \"with \"spaces 333"));
            Assert.Throws<Exception>(() => parser.SplitArgs("xxx xxx.yyy \"with \"spaces\" 333"));
        }
    }
}
