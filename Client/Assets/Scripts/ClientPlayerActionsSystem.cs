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
    public class ClientInputSystem : ComponentSystem
    {
        private EntityQuery playerInputStateQuery;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            playerInputStateQuery = EntityManager.CreateEntityQuery(ComponentType.ReadWrite<PlayerInputState>());
        }

        protected override void OnUpdate()
        {
            var playerInputState = playerInputStateQuery.GetSingleton<PlayerInputState>();
         
            playerInputState.selectUnitButtonPressed = Input.GetMouseButtonUp(0);
            playerInputState.actionButtonPressed = Input.GetMouseButtonUp(1);

            playerInputStateQuery.SetSingleton(playerInputState);
        }
    }
    
    public class ClientPlayerActionsSystem : ComponentSystem
    {
        private EntityQuery playerInputStateQuery;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            playerInputStateQuery = 
                EntityManager.CreateEntityQuery(ComponentType.ReadWrite<PlayerInputState>());
        }
        
        protected override void OnUpdate()
        {
            var playerInputState = playerInputStateQuery.GetSingleton<PlayerInputState>();
            
            var query = Entities.WithAll<Selectable, UnitComponent>().ToEntityQuery();

            var entities = query.ToEntityArray(Allocator.TempJob);
            var selectables = query.ToComponentDataArray<Selectable>(Allocator.TempJob);
            var units = query.ToComponentDataArray<UnitComponent>(Allocator.TempJob);

            var selectButtonPressed = playerInputState.selectUnitButtonPressed;
            var actionButtonPressed = playerInputState.actionButtonPressed;

            var spawnWaitingForPosition = playerInputState.spawnWaitingForPosition;
            
            Entities.ForEach(delegate(ref NetworkPlayerId networkPlayerId)
            {
                if (!networkPlayerId.connection.IsCreated)
                    return;
                
                // if (!networkPlayerId.assigned)
                //     return;
                
                // TODO: better controls...

                if (spawnWaitingForPosition)
                {
                    Entities.WithAll<UnitComponent, Selectable>().ForEach(delegate(ref UnitComponent unit)
                    {
                        unit.isSelected = false;
                    });

                    if (selectButtonPressed)
                    {
                        // send deplyo action...
                        
                    }
                }
                
                if (selectButtonPressed && !spawnWaitingForPosition)
                {
                    // Select a unit...
                    
                    var mousePosition = Input.mousePosition;
                    var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    worldPosition.z = 0;
                    
                    var bestSelectable = -1;
                    var bestDistance = 99999.0f;
                    
                    for (var i = 0; i < selectables.Length; i++)
                    {
                        var unit = units[i];
                        if (!unit.isActivePlayer)
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
                    var mousePosition = Input.mousePosition;
                    var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    var player = networkPlayerId.player;

                    if (spawnWaitingForPosition)
                    {
                        spawnWaitingForPosition = false;
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
                                    player = (byte) player,
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
                            
                            // Create Confirm feedback here...
                        
                            // unit.isSelected = false;
                        });   
                    }
                }
            });
            
            entities.Dispose();
            selectables.Dispose();
            units.Dispose();

            playerInputState.spawnWaitingForPosition = spawnWaitingForPosition;
            playerInputStateQuery.SetSingleton(playerInputState);
        }
    }

    public class ConfirmActionFeedbackSystem : ComponentSystem
    {
        private EntityQuery clientPrefabsQuery;
        protected override void OnCreate()
        {
            base.OnCreate();
            clientPrefabsQuery = EntityManager.CreateEntityQuery(
                ComponentType.ReadWrite<ClientPrefabsSharedComponent>());
        }

        protected override void OnUpdate()
        {
            var clientPrefabsEntity = clientPrefabsQuery.GetSingletonEntity();
            var clientPrefabs = EntityManager.GetSharedComponentData<ClientPrefabsSharedComponent>(clientPrefabsEntity);
            
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