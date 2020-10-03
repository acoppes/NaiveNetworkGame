using UnityEngine;

namespace Development
{
    [ExecuteInEditMode]
    public class RoundChildrenPosition : MonoBehaviour
    {
        public bool disabled = true;

        public float multiplier = 100;

        // public bool recursive;
        
        private void Update()
        {
            if (disabled)
                return;

            for (int i = 0; i < transform.childCount; i++)
            {
                var childTransform = transform.GetChild(i);
                var p = childTransform.localPosition;
                p = new Vector3(Mathf.RoundToInt(p.x * multiplier) / multiplier, 
                    Mathf.RoundToInt(p.y * multiplier) / multiplier, 
                    Mathf.RoundToInt(p.z * multiplier) / multiplier);
                childTransform.localPosition = p;
            }
        }
    }
}
