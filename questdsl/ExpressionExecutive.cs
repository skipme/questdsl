using System;
using System.Collections.Generic;

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
            AssignAdd,        // a=b+c
            AssignSub,        // a=b-c
            AssignMul,        // a=b*c
            AssignDiv,        // a=b/c
            AssignModulo,     // a=b%c

            Increment,        // ++
            Decrement,        // --

            Add,              // +=
            Subtract,         // -=

            Invocation,       // -->a x y z

            ToList,           // -->toList stateName (then substate values accessed by $list1, $list2...)
        }
        public ExpressionValue AssignVar;
        public ExecuteType FuncType;
        public ExpressionValue ExLeftPart;
        public ExpressionValue ExRightPart;

        public string InvokeTransition;
        public List<ExpressionValue> InvokeArgs;

        public ExpressionExecutive(string invokeName, List<ExpressionValue> argsList)
        {
            FuncType = ExecuteType.Invocation;
            InvokeTransition = invokeName;
            InvokeArgs = argsList;

            if (argsList == null || invokeName == null)
                throw new Exception();
        }
        public ExpressionExecutive(ExpressionValue assignVar, ExecuteType func, ExpressionValue left, ExpressionValue right)
        {
            if (assignVar == null || left == null || right == null)
                throw new Exception();

            if (assignVar.TypeOfReference == ExpressionValue.RefType.NotReferred || assignVar.TypeOfReference == ExpressionValue.RefType.Null)
                throw new Exception();
            this.AssignVar = assignVar;

            List<ExecuteType> etchck = new List<ExecuteType>() {
                ExecuteType.AssignAdd,
                ExecuteType.AssignDiv,
                ExecuteType.AssignModulo,
                ExecuteType.AssignMul,
                ExecuteType.AssignSub};
            FuncType = func;
            if (!etchck.Contains(func))
                throw new Exception();

            ExLeftPart = left;
            ExRightPart = right;
        }
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
                    if (left.TypeValue == ExpressionValue.ValueType.number
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
                    throw new Exception();
                    break;
                case ExecuteType.ToList:
                    break;
                default:
                    throw new Exception();
                    break;
            }

            FuncType = func;
            ExLeftPart = left;
            ExRightPart = right;
        }
    }
}