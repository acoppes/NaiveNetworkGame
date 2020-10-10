using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class WanderAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float range;
        
        public void Convert(Entity entity, EntityManager em, GameObjectConversionSystem conversionSystem)
        {
            em.AddComponentData(entity, new WanderArea()
            {
                range = range
            });
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}