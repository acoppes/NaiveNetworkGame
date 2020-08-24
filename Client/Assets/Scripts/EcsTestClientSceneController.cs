using System;
using System.Collections.Generic;
using Client;
using Mockups;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using Unity.Transforms;
using UnityEngine;

namespace Scenes
{
    // for when there a unit was updated from the server
    public struct NetworkGameStateUpdate : IComponentData
    {
        // public uint connectionId;
        public int frame;
        public int unitId;
        public int playerId;
        public float2 translation;
        public float2 lookingDirection;
        public int state;
    }

    public struct UnitComponent : IComponentData
    {
        public uint unitId;
    }
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ClientViewSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // iterate over client view updates...

            var query = Entities.WithAll<NetworkGameStateUpdate>().ToEntityQuery();
            
            var updates = query.ToComponentDataArray<NetworkGameStateUpdate>(Allocator.TempJob);
            var updateEntities = query.ToEntityArray(Allocator.TempJob);

            var unitsQuery = Entities.WithAll<UnitComponent, Translation>().ToEntityQuery();
            var units = unitsQuery.ToComponentDataArray<UnitComponent>(Allocator.TempJob);
            var unitEntities = unitsQuery.ToEntityArray(Allocator.TempJob);
            
            var createdUnitsInThisUpdate = new List<int>();

            for (var j = 0; j < updates.Length; j++)
            {
                PostUpdateCommands.DestroyEntity(updateEntities[j]);
                
                var update = updates[j];
                var updated = false;

                if (createdUnitsInThisUpdate.Contains(update.unitId))
                    continue;

                for (var i = 0; i < units.Length; i++)
                {
                    var unit = units[i];
                   
                    if (unit.unitId == update.unitId)
                    {
                        PostUpdateCommands.SetComponent(unitEntities[i], new Translation
                        {
                            Value = new float3(update.translation.x, update.translation.y, 0)
                        });
                        PostUpdateCommands.SetComponent(unitEntities[i], new UnitState
                        {
                            state = update.state
                        });
                        PostUpdateCommands.SetComponent(unitEntities[i], new LookingDirection
                        {
                            direction = update.lookingDirection
                        });
                        
                        updated = true;
                        break;
                    }
                }

                if (updated)
                    continue;
                
                var modelProvider = ModelProviderSingleton.Instance;
                    
                // create visual model for this unit
                var entity = PostUpdateCommands.CreateEntity();
                PostUpdateCommands.AddComponent(entity, new UnitComponent
                {
                    unitId = (uint) update.unitId
                });
                PostUpdateCommands.AddSharedComponent(entity, new ModelPrefabComponent
                {
                    prefab = modelProvider.prefabs[UnityEngine.Random.Range(0, modelProvider.prefabs.Length)]
                });
                PostUpdateCommands.AddComponent(entity, new Translation
                {
                    Value = new float3(update.translation.x, update.translation.y, 0)
                });
                PostUpdateCommands.AddComponent(entity, new UnitState
                {
                    state = update.state
                });
                PostUpdateCommands.AddComponent(entity, new LookingDirection
                {
                    direction = update.lookingDirection
                });
                // PostUpdateCommands.AddComponent(entity, new ClientConnectionId
                // {
                //     id = update.connectionId
                // });
                
                createdUnitsInThisUpdate.Add(update.unitId);
            }

            unitEntities.Dispose();
            updateEntities.Dispose();
            units.Dispose();
            updates.Dispose();
        }
    }

    // public struct ClientModelRootSharedComponent : ISharedComponentData, IEquatable<ClientModelRootSharedComponent>
    // {
    //     public Transform parent;
    //
    //     public bool Equals(ClientModelRootSharedComponent other)
    //     {
    //         return Equals(parent, other.parent);
    //     }
    //
    //     public override bool Equals(object obj)
    //     {
    //         return obj is ClientModelRootSharedComponent other && Equals(other);
    //     }
    //
    //     public override int GetHashCode()
    //     {
    //         return (parent != null ? parent.GetHashCode() : 0);
    //     }
    // }

    public struct NetworkPlayerId : IComponentData
    {
        public uint player;
        public NetworkConnection connection;
    }

    public class ClientInputSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(delegate(ref NetworkPlayerId networkPlayerId)
            {
                if (!networkPlayerId.connection.IsCreated)
                    return;
                
                // if (!networkPlayerId.assigned)
                //     return;
                
                // TODO: better controls...

                if (Input.GetMouseButtonUp(0))
                {
                    var mousePosition = Input.mousePosition;
                    var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

                    var e = PostUpdateCommands.CreateEntity();
                    PostUpdateCommands.AddComponent(e, new ClientOnly());
                    PostUpdateCommands.AddComponent(e, new PendingPlayerAction
                    {
                        player = networkPlayerId.player,
                        command = 0,
                        target = new float2(worldPosition.x, worldPosition.y)
                    });
                }
            });
            
           
        }
    }
    
    public class EcsTestClientSceneController : MonoBehaviour
    {
        public Camera camera;

        // public GameObject prefab;
        public Transform parent;

        private void Start()
        {
            // var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            // var clientModelRoot = entityManager.CreateEntity();
            // entityManager.AddSharedComponentData(clientModelRoot, new ClientModelRootSharedComponent
            // {
            //     parent = parent
            // });

            ModelProviderSingleton.Instance.SetRoot(parent);
        }
    }
}
