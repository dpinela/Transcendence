using RCLogic = RandomizerCore.Logic;
using RMStateVariables = RandomizerMod.RC.StateVariables;

namespace Transcendence
{
    internal class EquipTCharmVariable : RMStateVariables.EquipCharmVariable
    {
        private readonly RCLogic.Term CostTerm;

        public EquipTCharmVariable(string term, string charmName, int charmID, RCLogic.LogicManager lm)
            : base(term, charmName, charmID, lm) {
            CostTerm = lm.GetTermStrict(charmName + Transcendence.CostTermSuffix);
        }
        
        public override int GetNotchCost<T>(RCLogic.ProgressionManager pm, T state) => pm.Get(CostTerm.Id);
    }
}