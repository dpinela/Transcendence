using RandomizerCore.Logic;

namespace Transcendence
{
    internal class FuncVariableResolver : VariableResolver
    {
        internal delegate bool VRFunc(LogicManager lm, string term, out LogicVariable lvar);

        private readonly VRFunc func;

        public FuncVariableResolver(VRFunc f)
        {
            func = f;
        }

        public override bool TryMatch(LogicManager lm, string term, out LogicVariable lvar)
        {
            return func(lm, term, out lvar);
        }
    }
}