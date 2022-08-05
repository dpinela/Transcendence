using ItemChanger;
using ItemChanger.UIDefs;
using ItemChanger.Tags;

namespace Transcendence
{
    internal class FloristsBlessingRepairItem : AbstractItem
    {
        public FloristsBlessingRepairItem()
        {
            name = "Florist's_Blessing_Repair";
            UIDef = new MsgUIDef() { 
                name = new BoxedString("Florist's Blessing Repair"),
                shopDesc = new BoxedString(Description),
                sprite = new EmbeddedSprite() { key = "FloristsBlessing.png" }
            };
            tags = new()
            { 
                new PersistentItemTag() { Persistence = ItemChanger.Persistence.Persistent },
                new CompletionWeightTag() { Weight = 0 }
            };
        }

        private const string Description = "How could you ruin this one-of-a-kind flower? Here, I'll give you another one. This is the last one, I swear!";

        public override void GiveImmediate(GiveInfo info)
        {
            FloristsBlessing.Instance.Broken = false;
            Transcendence.UpdateNailDamage();
        }
    } 
}