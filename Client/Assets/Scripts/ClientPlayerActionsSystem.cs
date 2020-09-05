using Client;
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
                            }
                        
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
}