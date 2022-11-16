using UnityEngine;

namespace Transcendence
{
    internal class PositionSync : MonoBehaviour
    {
        public GameObject dest;

        public void Update()
        {
            dest.transform.position = gameObject.transform.position;
        }
    }
}