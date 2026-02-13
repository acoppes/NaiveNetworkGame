using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateBefore(typeof(UpdateNetworkGameStateSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct ProcessPendingPlayerActionsSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            // process all player pending actions
            foreach (var (pendingAction, playerController, e) in 
                SystemAPI.Query<RefRO<PendingPlayerAction>, RefRW<PlayerController>>()
                    .WithAll<ServerOnly>()
                    .WithEntityAccess())
            {
                var player = pendingAction.ValueRO.player;
                
                // Changed to only process build unit actions
                if (pendingAction.ValueRO.actionType != PlayerActionDefinition.BuildUnit)
                    continue;
                
                ecb.RemoveComponent<PendingPlayerAction>(e);

                var playerActions = state.EntityManager.GetBuffer<PlayerActionDefinition>(e);
                var playerAction = playerActions[pendingAction.ValueRO.unitType];

                // can't execute action if not enough gold...

                var prefab = playerAction.prefab;
                
                var unitComponent = state.EntityManager.GetComponentData<Unit>(prefab);

                // dont create unit if at maximum capacity
                if (unitComponent.slotCost > 0 && 
                    playerController.ValueRO.currentUnits + unitComponent.slotCost > playerController.ValueRO.maxUnits) 
                    continue;

                if (playerController.ValueRO.gold < playerAction.cost)
                    continue;

                var availableSlotIndex = 0;

                if (unitComponent.isBuilding)
                {
                    if (playerController.ValueRO.availableBuildingSlots == 0)
                        continue;

                    var actionProcessed = false;
                    
                    foreach (var (holder, buildingUnit, buildingEntity) in 
                        SystemAPI.Query<RefRO<BuildingHolder>, RefRO<Unit>>()
                            .WithNone<BuildUnitAction>()
                            .WithEntityAccess())
                    {
                        if (buildingUnit.ValueRO.player != player)
                            continue;
                        
                        // don't allow other barracks to process this action...
                        if (actionProcessed)
                            continue;

                        if (holder.ValueRO.hasBuilding)
                            continue;
                        
                        // enqueue unit build...
                        // consume player gold...

                        ecb.AddComponent(buildingEntity, new BuildUnitAction
                        {
                            // duration = b.spawnDuration,
                            prefab = prefab
                        });

                        actionProcessed = true;
                    }

                    if (actionProcessed)
                    {
                        playerController.ValueRW.gold -= playerAction.cost;
                    }
                    
                }
                else
                {
                    // search barrack capable of building this unit...
                    var actionProcessed = false;

                    var wanderArea = playerController.ValueRO.defendArea;
                    
                    foreach (var (barracks, barrackUnit, barrackEntity) in 
                        SystemAPI.Query<RefRO<Barracks>, RefRO<Unit>>()
                            .WithNone<BuildUnitAction>()
                            .WithEntityAccess())
                    {
                        if (barrackUnit.ValueRO.player != player)
                            continue;
                        
                        // don't allow other barracks to process this action...
                        if (actionProcessed)
                            continue;
                        
                        // check if this barracks can build this kind of units...
                        if (barracks.ValueRO.unitType != unitComponent.type)
                            continue;
                        
                        // enqueue unit build...
                        // consume player gold...
                        
                        ecb.AddComponent(barrackEntity, new BuildUnitAction
                        {
                            // duration = b.spawnDuration,
                            prefab = prefab,
                            wanderArea = wanderArea
                        });

                        actionProcessed = true;
                    }

                    if (actionProcessed)
                    {
                        playerController.ValueRW.gold -= playerAction.cost;
                    }
                }
            }
            
            foreach (var (pendingAction, e) in 
                SystemAPI.Query<RefRO<PendingPlayerAction>>()
                    .WithAll<PlayerController>()
                    .WithAll<PlayerBehaviour>()
                    .WithAll<ServerOnly>()
                    .WithEntityAccess())
            {
                // Changed to only process build unit actions
                if (pendingAction.ValueRO.actionType == PlayerActionDefinition.Attack)
                {
                    ecb.AddComponent<SwitchToAttackAction>(e);
                    ecb.RemoveComponent<PendingPlayerAction>(e);
                    
                } 
                else if (pendingAction.ValueRO.actionType == PlayerActionDefinition.Defend)
                {
                    ecb.AddComponent<SwitchToDefendAction>(e); 
                    ecb.RemoveComponent<PendingPlayerAction>(e);
                }
            }
            
            // Destroy the other pending actions not processed...
            var remainingPendingQuery = SystemAPI.QueryBuilder()
                .WithAll<ServerOnly, PendingPlayerAction, PlayerController, LocalTransform>()
                .Build();
            state.EntityManager.RemoveComponent<PendingPlayerAction>(remainingPendingQuery);
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
