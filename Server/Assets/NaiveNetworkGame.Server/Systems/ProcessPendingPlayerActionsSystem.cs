using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateBefore(typeof(UpdateNetworkGameStateSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class ProcessPendingPlayerActionsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // process all player pending actions
            Entities
                .WithAll<ServerOnly, PendingPlayerAction, PlayerController>()
                .ForEach(delegate (Entity e, ref PendingPlayerAction p, ref PlayerController playerController)
                {
                    var player = p.player;
                    
                    // Changed to only process build unit actions
                    if (p.actionType != PlayerAction.BuildUnit)
                        return;
                    
                    PostUpdateCommands.RemoveComponent<PendingPlayerAction>(e);

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
                        var wanderArea = playerController.defendArea;
                        
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
            
              Entities
                .WithAll<ServerOnly, PendingPlayerAction, PlayerController, PlayerBehaviour>()
                .ForEach(delegate (Entity e, ref PendingPlayerAction p, ref PlayerController playerController, ref PlayerBehaviour b)
                {
                    var player = playerController.player;
                    var attackArea = playerController.attackArea;
                    var defendArea = playerController.defendArea;
                    
                    // TODO: extract switch mode process to another system...
                    
                    // Changed to only process build unit actions
                    if (p.actionType == PlayerAction.Attack)
                    {
                        // switch player mode...
                        // create a switch mode action so player can do more stuff?
                        b.mode = 1;
                        // consume action
                        
                        Entities.WithAll<Unit, UnitBehaviour>()
                            .ForEach(delegate(Entity unitEntity, ref Unit u, ref UnitBehaviour ub)
                            {
                                if (u.player != player)
                                    return;
                                ub.wanderArea = attackArea;
                            });
                        
                        Entities.WithAll<Unit, IdleAction>()
                            .ForEach(delegate(Entity unitEntity, ref Unit u)
                            {
                                if (u.player != player)
                                    return;
                                PostUpdateCommands.RemoveComponent<IdleAction>(unitEntity);
                            });
                        
                        Entities.WithAll<Unit, MovementAction>()
                            .ForEach(delegate(Entity unitEntity, ref Unit u)
                            {
                                if (u.player != player)
                                    return;
                                PostUpdateCommands.RemoveComponent<MovementAction>(unitEntity);
                            });
                        
                        PostUpdateCommands.RemoveComponent<PendingPlayerAction>(e);
                    } else if (p.actionType == PlayerAction.Defend)
                    {
                        // switch player mode...
                        b.mode = 0;
                        // consume action
                        
                        Entities.WithAll<Unit, UnitBehaviour>()
                            .ForEach(delegate(Entity unitEntity, ref Unit u, ref UnitBehaviour ub)
                            {
                                if (u.player != player)
                                    return;
                                ub.wanderArea = defendArea;
                            });
                        
                        Entities.WithAll<Unit, IdleAction>()
                            .ForEach(delegate(Entity unitEntity, ref Unit u)
                            {
                                if (u.player != player)
                                    return;
                                PostUpdateCommands.RemoveComponent<IdleAction>(unitEntity);
                            });
                        
                        Entities.WithAll<Unit, MovementAction>()
                            .ForEach(delegate(Entity unitEntity, ref Unit u)
                            {
                                if (u.player != player)
                                    return;
                                PostUpdateCommands.RemoveComponent<MovementAction>(unitEntity);
                            });
                        
                        PostUpdateCommands.RemoveComponent<PendingPlayerAction>(e);
                    }
                    
                });
              
              Entities
                  .WithAll<ServerOnly, PendingPlayerAction, PlayerController, Translation>()
                  .ForEach(delegate (Entity e, ref PendingPlayerAction p, ref PlayerController playerController, ref Translation t)
                  {
                      // destroy the other pending actions not processed...
                      PostUpdateCommands.RemoveComponent<PendingPlayerAction>(e);
                  });
        }
    }
}