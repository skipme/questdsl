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
        public IEnumerable<string> GetVarsInScope()
        {
            if (ExLeftPart.TypeOfValue != ExpressionValue.ValueType.Reference 
                || ExLeftPart.TypeOfReference == ExpressionValue.RefType.LocalVar)
            {
                foreach (var v in ExLeftPart.vars)
                {
                    yield return v;
                }
            }

            if (ExRightPart.TypeOfValue != ExpressionValue.ValueType.Reference 
                || ExLeftPart.TypeOfReference == ExpressionValue.RefType.LocalVar)
            {
                foreach (var v in ExRightPart.vars)
                {
                    yield return v;
                }
            }
        }
        public override string Compile()
        {
            throw new System.NotImplementedException();
        }
    }
}