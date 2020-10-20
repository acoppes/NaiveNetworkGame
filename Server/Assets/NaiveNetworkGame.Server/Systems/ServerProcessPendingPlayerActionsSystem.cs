using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateBefore(typeof(UpdateNetworkGameStateSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class ServerProcessPendingPlayerActionsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // process all player pending actions
            Entities
                .WithAll<ServerOnly, ClientPlayerAction, PlayerController, Translation>()
                .ForEach(delegate (Entity e, ref ClientPlayerAction p, ref PlayerController playerController, ref Translation t)
                {
                    var player = p.player;

                    PostUpdateCommands.RemoveComponent<ClientPlayerAction>(e);
                
                    // Changed to only process build unit actions
                    if (p.actionType != ClientPlayerAction.BuildUnit)
                        return;
                    
                    var position = t.Value;

                    var playerActions = GetBufferFromEntity<PlayerAction>()[e];
                    var playerAction = playerActions[p.unitType];

                    // can't execute action if not enough gold...


                    var prefab = playerAction.prefab;
                    
                    var unitComponent = GetComponentDataFromEntity<Unit>()[prefab];

                    // dont create unit if at maximum capacity
                    if (unitComponent.slotCost > 0 && 
                        playerController.currentUnits + unitComponent.slotCost > playerController.maxUnits) 
                        return;

                    if (playerController.gold < playerAction.cost)
                        return;

                    var availableSlotIndex = 0;

                    if (unitComponent.isBuilding)
                    {
                        if (playerController.availableBuildingSlots == 0)
                            return;
                    
                        // var buildingSlotBuffer = GetBufferFromEntity<BuildingSlot>()[e];
                        //
                        // for (var i = 0; i < buildingSlotBuffer.Length; i++)
                        // {
                        //     var buildingSlotFor = buildingSlotBuffer[i];
                        //     if (!buildingSlotFor.hasBuilding)
                        //     {
                        //         position = buildingSlotFor.position;
                        //         
                        //         // buildingSlot.available = false;
                        //         // buildingSlotBuffer[i] = buildingSlot;
                        //         
                        //         availableSlotIndex = i;
                        //         
                        //         break;
                        //     }
                        // }
                        //
                        // playerController.gold -= playerAction.cost;
                        //
                        // var unitEntity = PostUpdateCommands.Instantiate(prefab);
                        //
                        // // Update the selected building slot with the entity....
                        // var buildingSlot = buildingSlotBuffer[availableSlotIndex];
                        // // buildingSlot.building = unitEntity;
                        // buildingSlot.hasBuilding = true;
                        // buildingSlotBuffer[availableSlotIndex] = buildingSlot;
                        //
                        // unitComponent.id = NetworkUnitId.current++;
                        // unitComponent.player = player;
                        //
                        // PostUpdateCommands.SetComponent(unitEntity, unitComponent);
                        // PostUpdateCommands.AddComponent(unitEntity, new Skin
                        // {
                        //     type = playerController.skinType
                        // });
                        //
                        // PostUpdateCommands.SetComponent(unitEntity, new Translation
                        // {
                        //     Value = position
                        // });
                        // PostUpdateCommands.SetComponent(unitEntity, new UnitState
                        // {
                        //     state = UnitState.spawningState
                        // });
                        //
                        // // var wanderArea = playerController.playerWander;
                        // //
                        // // PostUpdateCommands.AddComponent(unitEntity, new UnitBehaviour
                        // // {
                        // //     wanderArea = wanderArea,
                        // //     minIdleTime = 1,
                        // //     maxIdleTime = 3
                        // // });
                        //
                        // PostUpdateCommands.AddComponent<NetworkUnit>(unitEntity);
                        // PostUpdateCommands.AddComponent(unitEntity, new NetworkGameState());
                        // PostUpdateCommands.AddComponent(unitEntity, new NetworkTranslationSync());
                        
                        var actionProcessed = false;

                        var pendingAction = p;
                        
                        // var wanderArea = playerController.playerWander;
                        
                        Entities
                            .WithNone<BuildUnitAction>()
                            .WithAll<BuildingHolder, Unit>()
                            .ForEach(delegate(Entity be, ref BuildingHolder holder)
                            {
                                // don't allow other barracks to process this action...
                                if (actionProcessed)
                                    return;

                                if (holder.hasBuilding)
                                    return;
                                
                                // enqueue unit build...
                                // consume player gold...

                                PostUpdateCommands.AddComponent(be, new BuildUnitAction
                                {
                                    // duration = b.spawnDuration,
                                    prefab = prefab
                                });
                                // PostUpdateCommands.AddComponent(be, pendingAction);

                                actionProcessed = true;
                            });

                        if (actionProcessed)
                        {
                            playerController.gold -= playerAction.cost;
                        }
                        
                    }
                    else
                    {
                        // search barrack capable of biulding this unit...
                        var actionProcessed = false;

                        var pendingAction = p;
                        var wanderArea = playerController.playerWander;
                        
                        Entities
                            .WithNone<BuildUnitAction>()
                            .WithAll<Barracks, Unit>()
                            .ForEach(delegate(Entity be, ref Barracks b)
                            {
                                // don't allow other barracks to process this action...
                                if (actionProcessed)
                                    return;
                                
                                // enqueue unit build...
                                // consume player gold...

                                // check if this barracks can build this kind of units...
                                if (b.unitType != unitComponent.type)
                                    return;
                                
                                PostUpdateCommands.AddComponent(be, new BuildUnitAction
                                {
                                    // duration = b.spawnDuration,
                                    prefab = prefab,
                                    wanderArea = wanderArea
                                });
                                // PostUpdateCommands.AddComponent(be, pendingAction);

                                actionProcessed = true;
                            });

                        if (actionProcessed)
                        {
                            playerController.gold -= playerAction.cost;
                        }
                    }
                });
        }
    }
}