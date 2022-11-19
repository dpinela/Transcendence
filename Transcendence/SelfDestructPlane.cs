using UnityEngine;

namespace Transcendence
{
    internal class SelfDestructPlane : MonoBehaviour
    {
        public float y;

        public void FixedUpdate()
        {
            if (gameObject.transform.position.y < y)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}