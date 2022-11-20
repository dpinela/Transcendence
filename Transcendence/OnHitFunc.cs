using UnityEngine;
using GlobalEnums;

namespace Transcendence
{
    internal class OnHitFunc : MonoBehaviour
    {
        public Action<GameObject> OnHit;

        public void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.layer == (int)PhysLayers.ENEMIES)
            {
                OnHit(collider.gameObject);
            }
        }
    }
}