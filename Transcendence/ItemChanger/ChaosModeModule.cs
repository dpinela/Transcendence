using ItemChanger;
using ItemChanger.Modules;

namespace Transcendence
{
    internal class ChaosModeModule : Module
    {
        public override void Initialize()
        {
            Events.OnEnterGame += ApplyChaosMode;
        }

        public override void Unload()
        {
            Events.OnEnterGame -= ApplyChaosMode;
        }

        private void ApplyChaosMode()
        {
            var settings = Transcendence.Instance.Settings;
            // only do the Chaos Mode setup upon creating the save;
            // we don't want to reroll Chaos Orb or re-add it to the equipped charm list
            // on future loads.
            if (!settings.ChaosMode)
            {
                var orb = settings.ChaosOrb;
                orb.Equipped = true;
                orb.Cost = 0;
                settings.ChaosMode = true;
                PlayerData.instance.EquipCharm(ChaosOrb.Instance.Num);
                ChaosOrb.Instance.RerollCharms();
                PlayerData.instance.CountCharms();
            }
        }
    }
}