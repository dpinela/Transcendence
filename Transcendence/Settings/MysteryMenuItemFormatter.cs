using MCMenuElements = MenuChanger.MenuElements;

namespace Transcendence
{
    /// A MenuItemFormatter that hides its title unless a certain condition is met.
    internal class MysteryMenuItemFormatter : MCMenuElements.MenuItemFormatter
    {
        public MCMenuElements.MenuItemFormatter Inner;
        public Func<bool> Revealed;

        public override string GetText(string prefix, object value) =>
            Inner.GetText(Revealed() ? prefix : "???", value);
    }
}