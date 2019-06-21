using System;

namespace questdsl
{
    public class ExpressionExecutive : Expression
    {
        private readonly bool IsModifySubstate;
        public override bool SubStateModifies => IsModifySubstate;

        public override string Compile()
        {
            throw new System.NotImplementedException();
        }

        public enum ExecuteType
        {
            Assign,           // a=b
            //AssignAdd,        // a=b+c
            //AssignMinus,      // a=b-c

            Increment,        // ++
            Decrement,        // --

            Add,              // +=
            Subtract,         // -=

            Invocation,       // -->a x y z

            ToList,           // -->toList stateName (then substate values accessed by $list1, $list2...)
        }
        public ExecuteType FuncType;
        public ExpressionValue ExLeftPart;
        public ExpressionValue ExRightPart;

        public ExpressionExecutive(ExecuteType func, ExpressionValue left, ExpressionValue right)
        {
            if (func != ExecuteType.ToList &&
                func != ExecuteType.Invocation)

            {
                IsModifySubstate = false;
            }
            else
            {
                IsModifySubstate = true;
            }

            switch (func)
            {
                case ExecuteType.Assign:
                case ExecuteType.Increment:
                case ExecuteType.Decrement:
                case ExecuteType.Add:
                case ExecuteType.Subtract:
                    if (left.TypeOfReference == ExpressionValue.RefType.Image
                        || left.TypeOfReference == ExpressionValue.RefType.Arg
                        || left.TypeOfReference == ExpressionValue.RefType.List
                        || left.TypeOfReference == ExpressionValue.RefType.Null)
                    {
                        throw new Exception("");
                    }
                    if(left.TypeValue == ExpressionValue.ValueType.number
                        || left.TypeValue == ExpressionValue.ValueType.string_text)
                    {
                        throw new Exception("");
                    }
                    if (func == ExecuteType.Add
                        || func == ExecuteType.Subtract)
                    {
                        if (right.TypeOfReference == ExpressionValue.RefType.Image
                         || right.TypeOfReference == ExpressionValue.RefType.Null)
                        {
                            throw new Exception("");
                        }
                    }
                    break;
                case ExecuteType.Invocation:
                    break;
                case ExecuteType.ToList:
                    break;
                default:
                    break;
            }

            FuncType = func;
            ExLeftPart = left;
            ExRightPart = right;
        }
    }
}