using NaiveNetworkGame.Common;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class UnitCustomAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        // public float damage;
        // public float range;
        public byte player;
        public float speed;
        public float health;
        public float spawnDuration = 0;
        
        public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
        {
            entityManager.AddComponentData(entity, new Unit
            {
                player = player
            });
            
            entityManager.AddComponentData(entity, new Movement
            {
                speed = speed
            });
            entityManager.AddComponentData(entity, new UnitState());
            // entityManager.AddComponentData(entity, new Attack
            // {
            //     damage = damage,
            //     range = range
            // });
            entityManager.AddComponentData(entity, new LookingDirection());
            entityManager.AddComponentData(entity, new Health
            {
                total = health,
                current = health
            });

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