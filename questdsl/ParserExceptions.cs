using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{

    [Serializable]
    public class ParserEvaluationException : Exception
    {
        public ParserEvaluationException() { }
        public ParserEvaluationException(string message) : base(message) { }
        public ParserEvaluationException(string message, Exception inner) : base(message, inner) { }
        protected ParserEvaluationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
