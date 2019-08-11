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

        private ExpressionSubStateDefinition() { }
        public static ExpressionSubStateDefinition ImageSubstate(string StateName, ExpressionValue initial)
        {
            ExpressionSubStateDefinition n = new ExpressionSubStateDefinition();
            n.StateName = StateName;
            n.SubStateName = "$image";
            n.initialValue = initial;
            if (initial.TypeOfValue == ExpressionValue.ValueType.StateName_SubstateRef ||
               initial.TypeOfValue == ExpressionValue.ValueType.StateRef_SubstateRef ||
               initial.TypeOfValue == ExpressionValue.ValueType.StateRef_SubstateName ||
               initial.TypeOfValue == ExpressionValue.ValueType.Reference)
            {
                if (initial.TypeOfReference != ExpressionValue.RefType.Null && initial.TypeOfReference != ExpressionValue.RefType.Image)
                    throw new Exception("");
            }

            return n;
        }
        public ExpressionSubStateDefinition(string StateName, string SubStateName, ExpressionValue initial)
        {
            this.StateName = StateName;
            this.SubStateName = SubStateName;
            initialValue = initial;
            if (initialValue.TypeOfValue == ExpressionValue.ValueType.StateName_SubstateRef ||
               initialValue.TypeOfValue == ExpressionValue.ValueType.StateRef_SubstateRef ||
               initialValue.TypeOfValue == ExpressionValue.ValueType.StateRef_SubstateName ||
               initialValue.TypeOfValue == ExpressionValue.ValueType.Reference

                 )
            {
                if (initialValue.TypeOfReference != ExpressionValue.RefType.Null)
                    throw new Exception("");
            }
            if (SubStateName.Contains('.') || SubStateName.Contains('$'))
            {
                throw new Exception("");
            }
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
