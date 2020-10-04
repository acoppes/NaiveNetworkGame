using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Client.Systems
{
    public class CreatePlayerActionsFromInputSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // var query = Entities.WithAll<Selectable, Unit, UnitState>().ToEntityQuery();
            //
            // var entities = query.ToEntityArray(Allocator.TempJob);
            // var selectables = query.ToComponentDataArray<Selectable>(Allocator.TempJob);
            // var units = query.ToComponentDataArray<Unit>(Allocator.TempJob);
            // var states = query.ToComponentDataArray<UnitState>(Allocator.TempJob);
            
            // var selectButtonPressed = playerInputState.selectUnitButtonPressed;
            // var actionButtonPressed = playerInputState.actionButtonPressed;

            // TODO: player input, player and networkplayer could be the same entity...
            
            Entities.ForEach(delegate(ref NetworkPlayerId networkPlayerId, ref LocalPlayerController p, ref PlayerInputState playerInputState)
            {
                if (networkPlayerId.state != NetworkConnection.State.Connected)
                    return;

                // if (!networkPlayerId.assigned)
                //     return;
                
                // TODO: better controls...

                if (playerInputState.spawnActionPressed)
                {
                    Entities
                        .WithAllReadOnly<Selectable>()
                        .WithAll<Unit>().ForEach(delegate(ref Unit unit)
                    {
                        unit.isSelected = false;
                    });
                    
                    // send deplyo action...
                    var e = PostUpdateCommands.CreateEntity();
                    PostUpdateCommands.AddComponent(e, new ClientPlayerAction
                    {
                        player = p.player,
                        unit = 0,
                        command = ClientPlayerAction.BuildUnit
                    });

                    playerInputState.spawnActionPressed = false;
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
            
            // entities.Dispose();
            // selectables.Dispose();
            // units.Dispose();
            // states.Dispose();
            
            // playerInputState.spawnActionPressed = spawnActionPressed;
            // SetSingleton(playerInputState);
            // playerInputStateQuery.SetSingleton(playerInputState);
        }
    }
}