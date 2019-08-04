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
        public ExpressionValue stepValue;
        public ExpressionValue runtimeValue;
        public ExpressionSubStateDefinition(string StateName, string left, ExpressionValue right)
        {
            this.StateName = StateName;
            SubStateName = left;
            initialValue = right;
            if (initialValue.TypeOfValue == ExpressionValue.ValueType.StateName_SubstateRef ||
               initialValue.TypeOfValue == ExpressionValue.ValueType.StateRef_SubstateRef ||
               initialValue.TypeOfValue == ExpressionValue.ValueType.StateRef_SubstateName
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
                return initialValue.TypeOfValue == ExpressionValue.ValueType.SubstateName;
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
