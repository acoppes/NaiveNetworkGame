using System;
using System.Collections.Generic;
using Client;
using Mockups;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Scenes
{
    // for when there a unit was updated from the server
    public struct ClientViewUpdate : IComponentData
    {
        public uint connectionId;
        public uint unitId;
        public int state;
        public float2 position;
        public float2 lookingDirection;
    }

    public struct ClientUnitComponent : IComponentData
    {
        public uint unitId;
    }
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ClientViewSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // iterate over client view updates...

            var query = Entities.WithAll<ClientViewUpdate>().ToEntityQuery();
            
            var updates = query.ToComponentDataArray<ClientViewUpdate>(Allocator.TempJob);
            var updateEntities = query.ToEntityArray(Allocator.TempJob);

            var unitsQuery = Entities.WithAll<ClientUnitComponent, Translation>().ToEntityQuery();
            var units = unitsQuery.ToComponentDataArray<ClientUnitComponent>(Allocator.TempJob);
            var unitEntities = unitsQuery.ToEntityArray(Allocator.TempJob);

            for (var j = 0; j < updates.Length; j++)
            {
                PostUpdateCommands.DestroyEntity(updateEntities[j]);
                
                var update = updates[j];
                var updated = false;

                for (var i = 0; i < units.Length; i++)
                {
                    var unit = units[i];
                   
                    if (unit.unitId == update.unitId)
                    {
                        PostUpdateCommands.SetComponent(unitEntities[i], new Translation
                        {
                            Value = new float3(update.position.x, update.position.y, 0)
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
                PostUpdateCommands.AddComponent(entity, new ClientUnitComponent
                {
                    unitId = update.unitId
                });
                PostUpdateCommands.AddSharedComponent(entity, new ModelPrefabComponent
                {
                    prefab = modelProvider.prefabs[UnityEngine.Random.Range(0, modelProvider.prefabs.Length)]
                });
                PostUpdateCommands.AddComponent(entity, new Translation
                {
                    Value = new float3(update.position.x, update.position.y, 0)
                });
                PostUpdateCommands.AddComponent(entity, new UnitState
                {
                    state = update.state
                });
                PostUpdateCommands.AddComponent(entity, new LookingDirection
                {
                    direction = update.lookingDirection
                });
                PostUpdateCommands.AddComponent(entity, new ClientConnectionId
                {
                    id = update.connectionId
                });
            }

            unitEntities.Dispose();
            updateEntities.Dispose();
            units.Dispose();
            updates.Dispose();
        }
    }

    public struct ClientModelRootSharedComponent : ISharedComponentData, IEquatable<ClientModelRootSharedComponent>
    {
        public uint networkPlayerId;
        public Transform parent;

        public bool Equals(ClientModelRootSharedComponent other)
        {
            return networkPlayerId == other.networkPlayerId && Equals(parent, other.parent);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientModelRootSharedComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) networkPlayerId * 397) ^ (parent != null ? parent.GetHashCode() : 0);
            }
        }
    }
    
    public class EcsTestClientSceneController : MonoBehaviour
    {
        public Camera camera;

        public uint networkPlayerId;
        public int button;

        // public GameObject prefab;
        public Transform parent;

        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var clientModelRoot = entityManager.CreateEntity();
            entityManager.AddSharedComponentData(clientModelRoot, new ClientModelRootSharedComponent
            {
                networkPlayerId = networkPlayerId,
                parent = parent
            });
        }

        private void Update()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // if (clientBehaviour.clientNetworkManager.networkPlayerId == -1)
            //     return;

            if (Input.GetMouseButtonUp(button))
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = camera.ScreenToWorldPoint(mousePosition);

                var entity = entityManager.CreateEntity(ComponentType.ReadOnly<ClientOnly>());
                entityManager.AddComponentData(entity, new PendingPlayerAction
                {
                    player = networkPlayerId,
                    command = 0,
                    target = new float2(worldPosition.x, worldPosition.y)
                });
            }
        }
    }
}
