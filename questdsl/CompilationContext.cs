using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public class CompilationContext
    {
        private StringBuilder ContentBuilder = new StringBuilder();
        public string Content
        {
            get
            {
                return ContentBuilder.ToString();
            }
        }

        public void PrintCS(string lines)
        {
            ContentBuilder.Append(lines);
        }
    }
}
