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

                        var actionProcessed = false;
                        
                        Entities
                            .WithNone<BuildUnitAction>()
                            .WithAll<BuildingHolder, Unit>()
                            .ForEach(delegate(Entity be, ref BuildingHolder holder, ref Unit u)
                            {
                                if (u.player != player)
                                    return;
                                
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
                            .ForEach(delegate(Entity be, ref Barracks b, ref Unit u)
                            {
                                if (u.player != player)
                                    return;
                                
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