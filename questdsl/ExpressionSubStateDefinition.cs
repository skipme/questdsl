using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public class ExpressionSubStateDefinition : Expression
    {
        public override bool SubStateModifies => false;

        public string StateName;
        public string SubStateName;
        public ExpressionValue Val;
        public ExpressionSubStateDefinition(string StateName, string left, ExpressionValue right)
        {
            this.StateName = StateName;
            SubStateName = left;
            Val = right;
            if (Val.TypeValue == ExpressionValue.ValueType.StateName_SubstateRef ||
               Val.TypeValue == ExpressionValue.ValueType.StateRef_SubstateRef ||
               Val.TypeValue == ExpressionValue.ValueType.StateRef_SubstateName
                 )
            {
                throw new Exception("");
            }
            if (left.Contains('.') || left.Contains('$'))
                throw new Exception("");
        }
        public bool IsByReferenceDefined
        {
            get
            {
                return Val.TypeValue == ExpressionValue.ValueType.SubstateName;
            }
        }
        public override string Compile()
        {
            throw new NotImplementedException();
        }
    }
}
