using ItemChanger;
using System.Collections;

namespace Transcendence
{
    // A geo cost that must be paid every time the item is obtained,
    // instead of just the first time.
    internal record RecurringGeoCost(int geo) : GeoCost(geo)
    {
        public override void OnPay()
        {
            base.OnPay();
            // ItemChanger always sets Paid = true right after calling OnPay
            // and doesn't offer any hooks to let us override that behaviour.
            // So we work around it by just waiting a frame and then setting
            // Paid = false.
            GameManager.instance.StartCoroutine(Reset());
        }

        private IEnumerator Reset()
        {
            yield return null;
            Paid = false;
        }
    }
}