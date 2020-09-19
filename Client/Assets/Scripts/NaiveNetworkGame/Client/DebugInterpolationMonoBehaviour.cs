using UnityEngine;

namespace NaiveNetworkGame.Client
{
    public class DebugInterpolationMonoBehaviour : MonoBehaviour
    {
        public Vector3 p0;
        public Vector3 p1;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(p0, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(p1, 0.1f);
        }
    }
}