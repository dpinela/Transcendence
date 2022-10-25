using RandomizerCore.Logic;

namespace Transcendence
{
    internal class FuncVariableResolver : VariableResolver
    {
        internal delegate bool VRFunc(LogicManager lm, string term, out LogicInt lvar);

        private readonly VRFunc func;

        public FuncVariableResolver(VRFunc f)
        {
            func = f;
        }

        public override bool TryMatch(LogicManager lm, string term, out LogicInt lvar)
        {
            return func(lm, term, out lvar);
        }
    }

    internal class FuncLogicInt : LogicInt
    {
        public override string Name { get; }
        private readonly Func<int> evalFunc;

        public FuncLogicInt(string name, Func<int> evalFunc)
        {
            Name = name;
            this.evalFunc = evalFunc;
        }

        public override int GetValue(object sender, ProgressionManager pm)
        {
            return evalFunc();
        }

        public override IEnumerable<Term> GetTerms() => Enumerable.Empty<Term>();
    }
}