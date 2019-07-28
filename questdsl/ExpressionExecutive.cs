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
        public ExpressionValue AssignResultVar;
        public ExecuteType FuncType;
        public ExpressionValue ExLeftPart;
        public ExpressionValue ExRightPart;

        public string InvokeTransitionName;
        public List<ExpressionValue> InvokeArgs;

        public IEnumerable<string> GetVarsUsed()
        {
            if (AssignResultVar != null)
            {
                foreach (var v in AssignResultVar.vars)
                {
                    yield return v;
                }
            }
            foreach (var v in GetVarsReaded())
            {
                yield return v;
            }
        }
        public IEnumerable<string> GetVarsReaded()
        {
            if (this.FuncType != ExecuteType.Assign
                && ExLeftPart != null)
            {
                if (ExLeftPart.TypeOfValue != ExpressionValue.ValueType.Reference)
                {
                    foreach (var v in ExLeftPart.vars)
                    {
                        yield return v;
                    }
                }
            }
            if (ExRightPart != null)
            {
                if (ExRightPart.TypeOfValue != ExpressionValue.ValueType.Reference)
                {
                    foreach (var v in ExRightPart.vars)
                    {
                        yield return v;
                    }
                }
            }
        }
        public string GetVarDefined()
        {
            if (this.FuncType >= ExecuteType.AssignAdd
                && this.FuncType <= ExecuteType.AssignModulo
                && AssignResultVar != null
                && AssignResultVar.TypeOfReference == ExpressionValue.RefType.LocalVar)
            {
                return AssignResultVar.Left;
            }
            if (ExLeftPart != null
                && this.FuncType == ExecuteType.Assign
                && ExLeftPart.TypeOfReference == ExpressionValue.RefType.LocalVar)
            {
                return ExLeftPart.Left;
            }
            return null;
        }
        public IEnumerable<string> GetVarsAssigned()
        {
            if (AssignResultVar != null
                && AssignResultVar.TypeOfReference == ExpressionValue.RefType.LocalVar)
            {
                yield return AssignResultVar.Left;
            }
            if (ExLeftPart != null
                && this.FuncType == ExecuteType.Assign
                && ExLeftPart.TypeOfReference == ExpressionValue.RefType.LocalVar)
            {
                yield return ExLeftPart.Left;
            }
        }
        public ExpressionExecutive(string invokeName, List<ExpressionValue> argsList)
        {
            FuncType = ExecuteType.Invocation;
            InvokeTransitionName = invokeName;
            InvokeArgs = argsList;

            if (argsList == null || invokeName == null)
                throw new Exception();

            if (string.Compare(invokeName, "toList", true) == 0)
            {
                if (argsList.Count != 1)
                    throw new Exception();

                FuncType = ExecuteType.ToList;
            }
        }
        public ExpressionExecutive(ExpressionValue assignVar, ExecuteType func, ExpressionValue left, ExpressionValue right)
        {
            if (assignVar == null || left == null || right == null)
                throw new Exception();

            if (assignVar.TypeOfReference == ExpressionValue.RefType.NotReferred || assignVar.TypeOfReference == ExpressionValue.RefType.Null)
                throw new Exception();
            this.AssignResultVar = assignVar;

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
                    if (left.TypeOfReference == ExpressionValue.RefType.Null
                        || left.TypeOfValue == ExpressionValue.ValueType.number
                        || left.TypeOfValue == ExpressionValue.ValueType.string_text)
                        throw new Exception();
                    break;
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
                    if (left.TypeOfValue == ExpressionValue.ValueType.number
                        || left.TypeOfValue == ExpressionValue.ValueType.string_text)
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