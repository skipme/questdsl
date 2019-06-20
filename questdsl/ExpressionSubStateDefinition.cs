using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public class ExpressionSubStateDefinition : Expression, IComparable<ExpressionSubStateDefinition>
    {
        public override bool SubStateModifies => false;

        public string StateName;
        public string SubStateName;
        public ExpressionValue initialValue;
        public ExpressionSubStateDefinition(string StateName, string left, ExpressionValue right)
        {
            this.StateName = StateName;
            SubStateName = left;
            initialValue = right;
            if (initialValue.TypeValue == ExpressionValue.ValueType.StateName_SubstateRef ||
               initialValue.TypeValue == ExpressionValue.ValueType.StateRef_SubstateRef ||
               initialValue.TypeValue == ExpressionValue.ValueType.StateRef_SubstateName
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
                return initialValue.TypeValue == ExpressionValue.ValueType.SubstateName;
            }
        }
        public override string Compile()
        {
            throw new NotImplementedException();
        }

        int IComparable<ExpressionSubStateDefinition>.CompareTo(ExpressionSubStateDefinition other)
        {
            return string.CompareOrdinal(other.SubStateName, this.SubStateName);
        }
    }
}
