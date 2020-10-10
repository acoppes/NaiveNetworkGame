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
        public byte unitType;
        public byte slotsCost = 1;
        
        public float speed;
        public float health;
        public float spawnDuration = 0;

        public bool networking;
        
        public void Convert(Entity entity, EntityManager em, GameObjectConversionSystem conversionSystem)
        {
            em.AddComponentData(entity, new Unit
            {
                player = player,
                type = unitType,
                slotCost = slotsCost,
                id = NetworkUnitId.current++
            });
            
            em.AddComponentData(entity, new Movement
            {
                speed = speed
            });
            em.AddComponentData(entity, new UnitState());
            // entityManager.AddComponentData(entity, new Attack
            // {
            //     damage = damage,
            //     range = range
            // });
            em.AddComponentData(entity, new LookingDirection());
            em.AddComponentData(entity, new Health
            {
                total = health,
                current = health
            });

            em.AddComponentData(entity, new IsAlive());
            em.AddComponentData(entity, new UnitBehaviour());

            if (spawnDuration > 0)
            {
                em.AddComponentData(entity, new SpawningAction
                {
                    duration = spawnDuration
                });
            }

            if (networking)
            {
                em.AddComponentData(entity, new NetworkGameState());
                em.AddComponentData(entity, new NetworkTranslationSync());
            }
        }
    }
}