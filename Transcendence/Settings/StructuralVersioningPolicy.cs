using System.Text;
using System.Security.Cryptography;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace Transcendence
{
    internal class StructuralVersioningPolicy : VersioningPolicy<Signature>
    {
        internal Func<RandoSettings> settingsGetter;

        public override Signature Version => new() { FeatureSet = FeatureSetForSettings(settingsGetter()) };

        private static List<string> FeatureSetForSettings(RandoSettings rs) =>
            SupportedFeatures.Where(f => f.feature(rs)).Select(f => f.name).ToList();

        public override bool Allow(Signature s) => s.FeatureSet.All(name => SupportedFeatures.Any(sf => sf.name == name));

        private static List<(Predicate<RandoSettings> feature, string name)> SupportedFeatures = new()
        {
            (rs => rs.AddCharms, "Add15Charms"),
            (rs => rs.IncreaseMaxCharmCostBy > 0, "IncreaseMaxCharmCost"),
            (rs => rs.Logic.AntigravityAmulet, "AntigravLogic"),
            (rs => rs.Logic.BluemothWings == GeoCharmLogicMode.OnWithGeo, "BluemothGeoLogic"),
            (rs => rs.Logic.BluemothWings == GeoCharmLogicMode.On, "BluemothLogic"),
            (rs => rs.Logic.SnailSoul, "SnailSoulLogic"),
            (rs => rs.Logic.SnailSlash, "SnailSlashLogic"),
            (rs => rs.Logic.MillibellesBlessing, "MillibelleLogic"),
            (rs => rs.Logic.NitroCrystal, "NitroLogic"),
            (rs => rs.Logic.VespasVengeance, "VespaLogic"),
            (rs => rs.Logic.Crystalmaster == GeoCharmLogicMode.OnWithGeo, "CrystalmasterGeoLogic"),
            (rs => rs.Logic.Crystalmaster == GeoCharmLogicMode.On, "CrystalmasterLogic"),
            (rs => rs.Logic.ChaosOrb != ChaosOrbMode.Off, "ChaosLogic"),
            (rs => rs.Logic.AnyEnabled(), "LogicHash:" + LogicHash())
        };

        private static string LogicHash()
        {
            if (!Transcendence.LogicAvailable())
            {
                return "NIL";
            }

            using var hash = SHA256.Create();
            using var hstream = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write);
            var modDir = Path.GetDirectoryName(typeof(StructuralVersioningPolicy).Assembly.Location);
            foreach (var name in new string[] { "LogicMacros.json", "LogicPatches.json", "ConnectionLogicPatches.json" })
            {
                using (var logicFile = File.OpenRead(Path.Combine(modDir, name)))
                {
                    logicFile.CopyTo(hstream);
                }
            }
            hash.TransformFinalBlock(new byte[] {}, 0, 0);
            return ToHex(hash.Hash);
        }

        private static string ToHex(byte[] stuff)
        {
            var sb = new StringBuilder(stuff.Length * 2);
            foreach (var b in stuff)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }

    internal struct Signature
    {
        public List<string> FeatureSet;
    }
}