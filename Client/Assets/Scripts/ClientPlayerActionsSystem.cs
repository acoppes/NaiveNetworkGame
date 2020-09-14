using System.Collections;
using Client;
using Common;
using Mockups;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes
{
    public class ClientPlayerActionsSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<PlayerInputState>();
        }
        
        protected override void OnUpdate()
        {
            var playerInputStateEntity = GetSingletonEntity<PlayerInputState>();
            var playerInputState = EntityManager.GetComponentData<PlayerInputState>(playerInputStateEntity);
            
            var query = Entities.WithAll<Selectable, UnitComponent, UnitState>().ToEntityQuery();

            var entities = query.ToEntityArray(Allocator.TempJob);
            var selectables = query.ToComponentDataArray<Selectable>(Allocator.TempJob);
            var units = query.ToComponentDataArray<UnitComponent>(Allocator.TempJob);
            var states = query.ToComponentDataArray<UnitState>(Allocator.TempJob);
            
            var selectButtonPressed = playerInputState.selectUnitButtonPressed;
            var actionButtonPressed = playerInputState.actionButtonPressed;

            var spawnActionPressed = playerInputState.spawnActionPressed;
            
            Entities.ForEach(delegate(ref NetworkPlayerId networkPlayerId)
            {
                var player = networkPlayerId.player;
                
                if (!networkPlayerId.connection.IsCreated)
                    return;
                
                var mousePosition = Input.mousePosition;
                var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                worldPosition.z = 0;
                
                // if (!networkPlayerId.assigned)
                //     return;
                
                // TODO: better controls...

                if (spawnActionPressed)
                {
                    Entities
                        .WithAllReadOnly<Selectable>()
                        .WithAll<UnitComponent>().ForEach(delegate(ref UnitComponent unit)
                    {
                        unit.isSelected = false;
                    });
                    
                    // send deplyo action...
                    var e = PostUpdateCommands.CreateEntity();
                    PostUpdateCommands.AddComponent(e, new ClientPlayerAction
                    {
                        player = player,
                        unit = 0,
                        command = ClientPlayerAction.CreateUnitAction,
                        target = new float2(worldPosition.x, worldPosition.y)
                    });

                    spawnActionPressed = false; 

                    return;
                }
                
                
                /*if (selectButtonPressed && !spawnActionPressed)
                {
                    // Select a unit...
                    var bestSelectable = -1;
                    var bestDistance = 99999.0f;
                    
                    for (var i = 0; i < selectables.Length; i++)
                    {
                        var unit = units[i];
                        if (!unit.isActivePlayer || states[i].state == UnitState.spawningState)
                            continue;
                        
                        var selectable = selectables[i];
                        var bounds = selectable.bounds;

                        var distance = Vector3.SqrMagnitude(bounds.center - worldPosition);
                        
                        if (bounds.Contains(worldPosition) && distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestSelectable = i;
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

                if (actionButtonPressed)
                {
                    if (spawnActionPressed)
                    {
                        spawnActionPressed = false;
                        // dont show feedback!!
                    }
                    else
                    {
                        Entities.WithAll<UnitComponent, Selectable>().ForEach(delegate(ref UnitComponent unit)
                        {
                            if (unit.isSelected)
                            {
                                var e = PostUpdateCommands.CreateEntity();
                                PostUpdateCommands.AddComponent(e, new ClientOnly());
                                PostUpdateCommands.AddComponent(e, new ClientPlayerAction
                                {
                                    player = player,
                                    unit = unit.unitId,
                                    command = ClientPlayerAction.MoveUnitAction,
                                    target = new float2(worldPosition.x, worldPosition.y)
                                });

                                var feedback = PostUpdateCommands.CreateEntity();
                                PostUpdateCommands.AddComponent(feedback, new ConfirmActionFeedback
                                {
                                    position = new float2(worldPosition.x, worldPosition.y)
                                });
                            }
                            
                            // unit.isSelected = false;
                        });   
                    }
                }*/
            });
            
            entities.Dispose();
            selectables.Dispose();
            units.Dispose();
            states.Dispose();
            
            playerInputState.spawnActionPressed = spawnActionPressed;
            SetSingleton(playerInputState);
            // playerInputStateQuery.SetSingleton(playerInputState);
        }
    }

    public class ConfirmActionFeedbackSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<ClientPrefabsSharedComponent>();
        }

        protected override void OnUpdate()
        {
            var singletonEntity = GetSingletonEntity<ClientPrefabsSharedComponent>();
            var clientPrefabs = EntityManager.GetSharedComponentData<ClientPrefabsSharedComponent>(singletonEntity);
            
            Entities
                .WithAll<ConfirmActionFeedback>()
                .ForEach(delegate(Entity e, ref ConfirmActionFeedback feedback)
                {
                    PostUpdateCommands.DestroyEntity(e);

                    var confirmActionFeedback = GameObject.Instantiate(clientPrefabs.confirmActionPrefab);
                    confirmActionFeedback.transform.position = new Vector3(feedback.position.x, feedback.position.y, 0);
                    confirmActionFeedback.AddComponent<TempMonobehaviourForCoroutines>()
                        .StartCoroutine(DestroyActionOnComplete(confirmActionFeedback));

                });
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
            
            GameObject.Destroy(actionInstance);
        }
    }
}