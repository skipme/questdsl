using System.Collections.Generic;

namespace questdsl
{
    public class ExpressionBool : Expression
    {
        public enum Operation
        {
            eq,
            neq,
            lteq,
            bteq,
            lt,
            bt
        }
        public Operation ExOperation;
        public ExpressionValue ExLeftPart;
        public ExpressionValue ExRightPart;

        public override bool SubStateModifies => false;

        public ExpressionBool(Operation op, ExpressionValue left, ExpressionValue right)
        {
            ExOperation = op;
            ExLeftPart = left;
            ExRightPart = right;

            if (left.TypeOfReference == ExpressionValue.RefType.NotReferred
                && right.TypeOfReference == ExpressionValue.RefType.NotReferred)
            {
                throw new System.Exception();
            }
        }
        public IEnumerable<string> GetVars()
        {
            foreach (var item in ExLeftPart.vars)
            {
                yield return item;
            }
            foreach (var item in ExRightPart.vars)
            {
                yield return item;
            }
        }
        public override string Compile()
        {
            throw new System.NotImplementedException();
        }
    }
}