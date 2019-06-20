using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace questdsl
{
    public class Section : Expression
    {
        public List<ExpressionBool> ProbesOr;
        public List<ExpressionExecutive> Body;

        
        public Section(List<ExpressionBool> probes, List<ExpressionExecutive> body)
        {
            ProbesOr = probes;
            Body = body;
            if (body == null || body.Count == 0)
                throw new Exception();

            IsSubStateModifies = modifiesTo().Any();
        }

        public IEnumerable<string> dependentOn()
        {
            if (ProbesOr == null)
                yield break;

            foreach (var item in ProbesOr)
            {
                if (item.ExLeftPart.TypeValue == ExpressionValue.ValueType.SubstateName)
                    yield return item.ExLeftPart.SubstatePath;
                else if (item.ExLeftPart.TypeValue == ExpressionValue.ValueType.StateRef_SubstateRef)
                    yield return $"{item.ExLeftPart.Left}.*";

                if (item.ExRightPart.TypeValue == ExpressionValue.ValueType.SubstateName)
                    yield return item.ExRightPart.SubstatePath;
                else if (item.ExRightPart.TypeValue == ExpressionValue.ValueType.StateRef_SubstateRef)
                    yield return $"{item.ExRightPart.Left}.*";
            }

        }
        public IEnumerable<string> modifiesTo()
        {
            foreach (var item in Body)
            {
                if (item.SubStateModifies)
                {
                    //foreach (var item in item.)
                    //{

                    //}
                }
            }
            yield break;
        }
        readonly bool IsSubStateModifies;
        public override bool SubStateModifies => IsSubStateModifies;

        public override string Compile()
        {
            throw new NotImplementedException();
        }
    }
}
