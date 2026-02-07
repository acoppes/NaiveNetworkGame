using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class WanderAreaAuthoring : MonoBehaviour
    {
        public float range;
        
        private class WanderAreaBaker : Baker<WanderAreaAuthoring>
        {
            public override void Bake(WanderAreaAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new WanderArea()
                {
                    range = authoring.range
                });
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}