using System;
using System.Collections.Generic;
using NaiveNetworkGame.Common;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class UnitCustomAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [Serializable]
        public struct Behaviour
        {
            public float minIdleTime;
            public float maxIdleTime;
            public GameObject wanderArea;
        }

        [Serializable]
        public struct DynamicObstacleData
        {
            public byte priority;
            public float range;
        }
        
        // public float damage;
        // public float range;
        
        public byte player;
        public byte unitType;
        public byte slotsCost = 1;
        
        public float speed;
        public float health;
        public float spawnDuration = 0;

        public bool isBuilding;

        public Behaviour behaviourData;
        public DynamicObstacleData dynamiceObstacleData;
        
        public bool networking;
        
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            if (behaviourData.wanderArea != null)
            {
                referencedPrefabs.Add(behaviourData.wanderArea);
            }
        }
        
        public void Convert(Entity entity, EntityManager em, GameObjectConversionSystem conversionSystem)
        {
            em.AddComponentData(entity, new Unit
            {
                player = player,
                type = unitType,
                slotCost = slotsCost,
                id = NetworkUnitId.current++,
                isBuilding = isBuilding
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

            em.AddComponentData(entity, new UnitBehaviour
            {
                minIdleTime = behaviourData.minIdleTime,
                maxIdleTime = behaviourData.maxIdleTime,
                wanderArea = behaviourData.wanderArea != null ? 
                    conversionSystem.GetPrimaryEntity(behaviourData.wanderArea) : Entity.Null
            });

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

            if (dynamiceObstacleData.priority > 0)
            {
                em.AddComponentData(entity, new DynamicObstacle
                {
                    priority = dynamiceObstacleData.priority,
                    range = dynamiceObstacleData.range
                });
            }
        }

        private void OnDrawGizmos()
        {
            if (dynamiceObstacleData.priority > 0)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, dynamiceObstacleData.range);
            }
        }
    }
}