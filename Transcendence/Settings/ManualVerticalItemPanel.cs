using MenuChanger;
using MenuChanger.MenuPanels;
using MenuChanger.MenuElements;
using UnityEngine;
using UnityEngine.UI;

namespace Transcendence
{
    internal class ManualVerticalItemPanel : IMenuPanel
    {
        private VerticalItemPanel panel;
        private Vector2 topCenterPos;
        private float[] vspaces;

        public ManualVerticalItemPanel(MenuPage mp, Vector2 topCenterPos, IMenuElement[] children, float[] vspaces)
        {
            panel = new VerticalItemPanel(mp, topCenterPos, 1, true, children);
            this.topCenterPos = topCenterPos;
            this.vspaces = vspaces;
            Reposition();
        }

        public void Add(IMenuElement obj) => panel.Items.Add(obj);

        public bool Remove(IMenuElement obj)
        {
            var found = panel.Items.Remove(obj);
            if (found)
            {
                Reposition();
            }
            return found;
        }

        public List<IMenuElement> Items => panel.Items;

        public MenuPage Parent => panel.Parent;
        public void Hide() => panel.Hide();
        public void Show() => panel.Show();
        public bool Hidden => panel.Hidden;
        public void Destroy() => panel.Destroy();

        public void MoveTo(Vector2 pos)
        {
            RepositionRelative(pos - topCenterPos);
            topCenterPos = pos;
        }

        public void Translate(Vector2 delta)
        {
            RepositionRelative(delta);
            topCenterPos += delta;
        }


        public void SetNeighbor(Neighbor n, ISelectable s) => panel.SetNeighbor(n, s);
        public ISelectable GetISelectable(Neighbor n) => panel.GetISelectable(n);
        public Selectable GetSelectable(Neighbor n) => panel.GetSelectable(n);

        public void ResetNavigation() => panel.ResetNavigation();

        private void Reposition()
        {
            float offset = 0;
            for (var i = 0; i < panel.Items.Count; i++)
            {
                offset += vspaces[i];
                panel.Items[i].MoveTo(topCenterPos + new Vector2(0, -offset));
            }
        }

        private void RepositionRelative(Vector2 delta)
        {
            foreach (var e in panel.Items)
            {
                e.Translate(delta);
            }
        }
    }
}