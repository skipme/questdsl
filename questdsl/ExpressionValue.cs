using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace questdsl
{
    public class ExpressionValue : Expression, ICloneable
    {
        public override bool SubStateModifies => false;

        public static readonly ExpressionValue NullValue = new ExpressionValue(ValueType.Reference, "null");
        public enum ValueType
        {
            Reference, // scope var
            SubstateName,
            StateName_SubstateRef,
            StateRef_SubstateName,
            StateRef_SubstateRef,
            string_text,
            number
        }
        public enum RefType
        {
            NotReferred,
            Substate,
            Null,
            Arg,
            List,
            Image,
            LocalVar
        }
        public string Left;
        public string Right;
        public string SubstatePath
        {
            get
            {
                return $"{Left}.{Right}";
            }
        }
        public int Num;
        public ValueType TypeOfValue;

        public RefType TypeOfReference = RefType.NotReferred;

        public int ArgOrListIndex;

        public ExpressionValue(ValueType valType, string left, string right = null, int number = -1)
        {
            TypeOfValue = valType;
            if (!Enum.IsDefined(typeof(ValueType), valType))
                throw new Exception();

            if ((new List<ValueType>() { ValueType.StateName_SubstateRef, ValueType.StateRef_SubstateName, ValueType.StateRef_SubstateRef, ValueType.SubstateName })
                .Contains(valType))
            {
                if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
                    throw new Exception();
            }
            vars = new List<string>();
            switch (valType)
            {
                case ValueType.Reference:
                    Match m = Regex.Match(left, @"^arg(\d+)$");
                    if (m.Success && m.Groups[1].Success)
                    {
                        ArgOrListIndex = Int32.Parse(m.Groups[1].Value) - 1;
                        TypeOfReference = RefType.Arg;
                    }
                    else
                    {
                        m = Regex.Match(left, @"^list(\d+)$");
                        if (m.Success && m.Groups[1].Success)
                        {
                            ArgOrListIndex = Int32.Parse(m.Groups[1].Value) - 1;
                            TypeOfReference = RefType.List;
                        }
                        else if (left == "null")
                            TypeOfReference = RefType.Null;
                        else if (left == "image")
                            TypeOfReference = RefType.Image;
                        else
                            TypeOfReference = RefType.LocalVar;
                    }
                    vars.Add(left);

                    break;
                case ValueType.SubstateName:
                    TypeOfReference = RefType.Substate;
                    break;
                case ValueType.StateName_SubstateRef:
                    TypeOfReference = RefType.Substate;
                    vars.Add(right);
                    break;
                case ValueType.StateRef_SubstateName:
                    TypeOfReference = RefType.Substate;
                    vars.Add(left);
                    break;
                case ValueType.StateRef_SubstateRef:
                    TypeOfReference = RefType.Substate;
                    vars.Add(left);
                    vars.Add(right);
                    break;
                case ValueType.string_text:
                    if (left.TrimStart().StartsWith("$"))
                        throw new Exception();
                    break;
                case ValueType.number:
                    break;
            }

            Left = left;
            Right = right;
            Num = number;
        }
        public List<string> vars;
        public override string Compile()
        {
            throw new System.NotImplementedException();
        }

        public object Clone()
        {
            return new ExpressionValue(this.TypeOfValue,
                Left, Right, Num);
        }
    }
}