using RCLogic = RandomizerCore.Logic;
using RMStateVariables = RandomizerMod.RC.StateVariables;

namespace Transcendence
{
    internal class EquipTCharmVariable : RMStateVariables.EquipCharmVariable
    {
        public EquipTCharmVariable(string term, string charmName, int charmID, RCLogic.LogicManager lm)
            : base(term, charmName, charmID, lm) {}
        
        public override int GetNotchCost<T>(RCLogic.ProgressionManager pm, T state) =>
            Transcendence.Instance.NextRandoNotchCosts[CharmID];
    }
}