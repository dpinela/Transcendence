using RCLogic = RandomizerCore.Logic;

namespace Transcendence
{
    internal class TVariableResolver : RCLogic.VariableResolver
    {
        public override bool TryMatch(RCLogic.LogicManager lm, string term, out RCLogic.LogicVariable lvar)
        {
            if (IsConditionalLogicTerm("EQUIPPED_TRANSCENDENCE_CHARM", term, out var charmName))
            {
                var num = Transcendence.Charms.First(c => c.Name.Replace(" ", "_") == charmName).Num;
                lvar = new EquipTCharmVariable(term, charmName, num, lm);
                return true;
            }
            if (IsConditionalLogicTerm("TrueOrNotExist", term, out var innerTerm))
            {
                if (Inner == null || !Inner.TryMatch(lm, innerTerm, out lvar))
                {
                    lvar = new RCLogic.ConstantInt(RCLogic.LogicVariable.TRUE);
                }
                return true;
            }
            if (IsConditionalLogicTerm("TrueAndExists", term, out var innerTermII))
            {
                if (Inner == null || !Inner.TryMatch(lm, innerTermII, out lvar))
                {
                    lvar = new RCLogic.ConstantInt(RCLogic.LogicVariable.FALSE);
                }
                return true;
            }
            return Inner.TryMatch(lm, term, out lvar);
        }

        private static bool IsConditionalLogicTerm(string condition, string term, out string innerTerm)
        {
            var startMarker = "$" + condition + "[";
            const string endMarker = "]";
            if (term.StartsWith(startMarker) && term.EndsWith(endMarker))
            {
                innerTerm = term.Substring(startMarker.Length, term.Length - startMarker.Length - endMarker.Length);
                return true;
            }
            innerTerm = "";
            return false;
        }
    }
}