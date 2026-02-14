using System;
using System.Collections.Generic;
using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class UnitCustomAuthoring : MonoBehaviour
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
        
        [Serializable]
        public struct BarracksData
        {
            public bool isBarrack;
            public byte unitType;
            public Transform spawnPosition;
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
        public BarracksData barracksData;

        public Behaviour behaviourData;
        public DynamicObstacleData dynamiceObstacleData;

        public bool isHolder;

        public bool networking;

        private class UnitCustomBaker : Baker<UnitCustomAuthoring>
        {
            public override void Bake(UnitCustomAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Unit
                {
                    player = authoring.player,
                    type = authoring.unitType,
                    slotCost = authoring.slotsCost,
                    // id = NetworkUnitId.current++,
                    isBuilding = authoring.isBuilding,
                    spawnDuration = authoring.spawnDuration
                });
                
                AddComponent(entity, new RequireNetworkUnitId());

                AddComponent(entity, new Skin());
                AddComponent(entity, new Movement
                {
                    speed = authoring.speed
                });
                AddComponent(entity, new UnitStateComponent());
                // entityManager.AddComponentData(entity, new Attack
                // {
                //     damage = damage,
                //     range = range
                // });
                AddComponent(entity, new LookingDirection
                {
                    direction = new float2(1, 0)
                });
                
                if (authoring.health > 0)
                {
                    AddComponent(entity, new Health
                    {
                        total = authoring.health,
                        current = authoring.health
                    });
                }

                AddComponent(entity, new IsAlive());

                if (!authoring.isBuilding && !authoring.isHolder)
                {
                    AddComponent(entity, new UnitBehaviourComponent
                    {
                        minIdleTime = authoring.behaviourData.minIdleTime,
                        maxIdleTime = authoring.behaviourData.maxIdleTime,
                        wanderArea = authoring.behaviourData.wanderArea != null
                            ? GetEntity(authoring.behaviourData.wanderArea, TransformUsageFlags.Dynamic)
                            : Entity.Null
                    });
                }

                if (authoring.spawnDuration > 0)
                {
                    AddComponent(entity, new SpawningAction
                    {
                        duration = authoring.spawnDuration
                    });
                }

                if (authoring.networking)
                {
                    AddComponent(entity, new NetworkGameState());
                    AddComponent(entity, new NetworkTranslationSync());
                }

                if (authoring.dynamiceObstacleData.priority > 0)
                {
                    AddComponent(entity, new DynamicObstacle
                    {
                        priority = authoring.dynamiceObstacleData.priority,
                        range = authoring.dynamiceObstacleData.range
                    });
                }

                if (authoring.barracksData.isBarrack)
                {
                    AddComponent(entity, new Barracks
                    {
                        unitType = authoring.barracksData.unitType,
                    });
                    AddComponent(entity, new UnitSpawnPosition
                    {
                        position = authoring.barracksData.spawnPosition.localPosition
                    });
                }
                
                if (authoring.isHolder)
                {
                    AddComponent(entity, new BuildingHolder
                    {
                        hasBuilding = false
                    });
                    AddComponent(entity, new UnitSpawnPosition
                    {
                        position = float3.zero
                    });
                }
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