using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class UnitCustomAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float spawnDuration = 0;
        
        public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
        {
            entityManager.AddComponentData(entity, new IsAlive());
            entityManager.AddComponentData(entity, new UnitBehaviour());

            if (spawnDuration > 0)
            {
                entityManager.AddComponentData(entity, new SpawningAction
                {
                    duration = spawnDuration
                });
            }
        }
    }
}