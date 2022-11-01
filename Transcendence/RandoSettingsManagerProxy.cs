using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace Transcendence
{
    internal class RandoSettingsManagerProxy : RandoSettingsProxy<RandoSettings, int>
    {
        internal Func<RandoSettings> getter;
        internal Action<RandoSettings> setter;

        public override string ModKey => nameof(Transcendence);

        public override VersioningPolicy<int> VersioningPolicy => new NoVersioningPolicy();

        public override bool TryProvideSettings(out RandoSettings? sent)
        {
            sent = getter();
            return true;
        }

        public override void ReceiveSettings(RandoSettings? received)
        {
            setter(received ?? new());
        }
    }
}