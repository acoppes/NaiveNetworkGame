using System;
using System.Collections;
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
        public float delta;
        public int unitId;
        public int playerId;
        public byte unitType;
        public float2 translation;
        public float2 lookingDirection;
        public int state;
    }

    // public struct UnitGameState : IBufferElementData
    // {
    //     public int frame;
    //     public float time;
    //     public float2 translation;
    // }

    public struct UnitComponent : IComponentData
    {
        public uint unitId;
        public uint player;
        public bool isActivePlayer;
        public bool isSelected;
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
            var translations = unitsQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var unitEntities = unitsQuery.ToEntityArray(Allocator.TempJob);
            
            var createdUnitsInThisUpdate = new List<int>();
            
            // first create entities to be updated....

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
                        // PostUpdateCommands.SetComponent(unitEntities[i], new Translation
                        // {
                        //     Value = new float3(update.translation.x, update.translation.y, 0)
                        // });
                        
                        PostUpdateCommands.SetComponent(unitEntities[i], new UnitState
                        {
                            state = update.state
                        });
                        PostUpdateCommands.SetComponent(unitEntities[i], new LookingDirection
                        {
                            direction = update.lookingDirection
                        });

                        var currentTranslation = translations[i];
                        
                        PostUpdateCommands.SetComponent(unitEntities[i], new UnitGameStateInterpolation
                        {
                            previousTranslation = currentTranslation.Value.xy,
                            currentTranslation = update.translation,
                            remoteDelta = update.delta,
                            time = 0
                        });

                        // var updateBuffer = PostUpdateCommands.SetBuffer<UnitGameState>(unitEntities[i]);
                        // buffer.Add(new UnitGameState
                        // {
                        //     frame = update.frame,
                        //     time = update.time,
                        //     translation = update.translation
                        // });
                        
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
                    unitId = (uint) update.unitId,
                    player = (uint) update.playerId
                });
                PostUpdateCommands.AddSharedComponent(entity, new ModelPrefabComponent
                {
                    prefab = modelProvider.prefabs[update.unitType]
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

                PostUpdateCommands.AddComponent(entity, new UnitGameStateInterpolation
                {
                    previousTranslation = update.translation,
                    currentTranslation = update.translation,
                    time = 0,
                    remoteDelta = update.delta
                });

                if (update.unitType == 0)
                {
                    PostUpdateCommands.AddComponent(entity, new Selectable());
                }
                
                // var buffer = PostUpdateCommands.AddBuffer<UnitGameState>(entity);
                // buffer.Add(new UnitGameState
                // {
                //     frame = update.frame,
                //     time = update.time,
                //     translation = update.translation
                // });
                
                // PostUpdateCommands.AddComponent(entity, new ClientConnectionId
                // {
                //     id = update.connectionId
                // });
                
                createdUnitsInThisUpdate.Add(update.unitId);
            }

            translations.Dispose();
            unitEntities.Dispose();
            updateEntities.Dispose();
            units.Dispose();
            updates.Dispose();
        }
    }

    public class UpdateUnitPlayerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // NetworkPlayerId
            
            Entities.ForEach(delegate(ref NetworkPlayerId networkPlayer)
            {
                var player = networkPlayer.player;
                
                Entities.ForEach(delegate(ref UnitComponent unit)
                {
                    unit.isActivePlayer = unit.player == player;
                });
                
            });
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
            var query = Entities.WithAll<Selectable, UnitComponent>().ToEntityQuery();

            var entities = query.ToEntityArray(Allocator.TempJob);
            var selectables = query.ToComponentDataArray<Selectable>(Allocator.TempJob);
            var units = query.ToComponentDataArray<UnitComponent>(Allocator.TempJob);
            
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
                    worldPosition.z = 0;
                    
                    var bestSelectable = -1;
                    
                    for (var i = 0; i < selectables.Length; i++)
                    {
                        var unit = units[i];
                        if (!unit.isActivePlayer)
                            continue;
                        
                        var selectable = selectables[i];
                        var bounds = selectable.bounds;
                        if (bounds.Contains(worldPosition))
                        {
                            bestSelectable = i;
                            break;
                        }
                    }

                    if (bestSelectable != -1)
                    {
                        var entity = entities[bestSelectable];
                        var unit = units[bestSelectable];
                        unit.isSelected = !unit.isSelected;
                        
                        Entities.WithAll<UnitComponent, Selectable>().ForEach(delegate(ref UnitComponent unit)
                        {
                            unit.isSelected = false;
                        });
                        
                        PostUpdateCommands.SetComponent(entity, unit);
                    }
                    else
                    {
                        Entities.WithAll<UnitComponent, Selectable>().ForEach(delegate(ref UnitComponent unit)
                        {
                            unit.isSelected = false;
                        });
                    }
                }

                if (Input.GetMouseButtonUp(1))
                {
                    var mousePosition = Input.mousePosition;
                    var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    var player = networkPlayerId.player;
                    
                    Entities.WithAll<UnitComponent, Selectable>().ForEach(delegate(ref UnitComponent unit)
                    {
                        if (unit.isSelected)
                        {
                            var e = PostUpdateCommands.CreateEntity();
                            PostUpdateCommands.AddComponent(e, new ClientOnly());
                            PostUpdateCommands.AddComponent(e, new PendingPlayerAction
                            {
                                player = player,
                                unit = unit.unitId,
                                command = 0,
                                target = new float2(worldPosition.x, worldPosition.y)
                            });
                        }
                        
                        // unit.isSelected = false;
                    });
                }
            });
            
            entities.Dispose();
            selectables.Dispose();
            units.Dispose();
        }
    }
    
    public class EcsTestClientSceneController : MonoBehaviour
    {
        public Camera camera;

        // public GameObject prefab;
        public Transform parent;
        
        public GameObject actionPrefab;

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

        private void Update()
        {
            if (Input.GetMouseButtonUp(1))
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

                worldPosition.z = 0;

                var actionInstance = Instantiate(actionPrefab, worldPosition, Quaternion.identity);
                StartCoroutine(DestroyActionOnComplete(actionInstance));
            }
        }

        private IEnumerator DestroyActionOnComplete(GameObject actionInstance)
        {
            var animator = actionInstance.GetComponent<Animator>();
            var hiddenState = Animator.StringToHash("Hidden");
            
            animator.SetTrigger("Action");

            yield return null;
            
            yield return new WaitUntil(delegate
            {
                var currentState = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
                return currentState == hiddenState;
            });
            
            Destroy(actionInstance);
        }
    }
}
