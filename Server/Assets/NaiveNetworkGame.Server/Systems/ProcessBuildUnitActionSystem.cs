using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ProcessPendingPlayerActionsSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct ProcessBuildUnitActionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
                
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var holder in 
                SystemAPI.Query<RefRW<BuildingHolder>>()
                    .WithAll<BuildUnitAction>())
            {
                holder.ValueRW.hasBuilding = true;
            }
            
            foreach (var (buildAction, spawnPosition, barrackUnit, localTransform, entity) in 
                SystemAPI.Query<RefRW<BuildUnitAction>, RefRO<UnitSpawnPosition>, RefRO<Unit>, RefRO<LocalTransform>>()
                    .WithAll<ServerOnly>()
                    .WithEntityAccess())
            {
                buildAction.ValueRW.time += dt;

                if (!buildAction.ValueRO.unitSpawning)
                {
                    buildAction.ValueRW.unitSpawning = true;
                    
                    var unitComponent = state.EntityManager.GetComponentData<Unit>(buildAction.ValueRO.prefab);
                    buildAction.ValueRW.duration = unitComponent.spawnDuration;

                    // delay? 
                    var unitEntity = ecb.Instantiate(buildAction.ValueRO.prefab);
                
                    unitComponent.id = NetworkUnitId.current++;
                    unitComponent.player = barrackUnit.ValueRO.player;
                
                    ecb.SetComponent(unitEntity, unitComponent);
                    ecb.SetComponent(unitEntity, new LocalTransform
                    {
                        Position = localTransform.ValueRO.Position + spawnPosition.ValueRO.position,
                        Rotation = Unity.Mathematics.quaternion.identity,
                        Scale = 1f
                    });
                
                    ecb.SetComponent(unitEntity, new UnitStateComponent
                    {
                        state = UnitStateTypes.spawningState
                    });
                
                    ecb.AddComponent(unitEntity, new SpawningAction
                    {
                        duration = buildAction.ValueRO.duration 
                    });

                    var wanderArea = buildAction.ValueRO.wanderArea;

                    ecb.AddComponent(unitEntity, new UnitBehaviourComponent
                    {
                        wanderArea = wanderArea,
                        minIdleTime = 1,
                        maxIdleTime = 3
                    });
                
                    ecb.AddComponent<NetworkUnit>(unitEntity);
                    ecb.AddComponent(unitEntity, new NetworkGameState());
                    ecb.AddComponent(unitEntity, new NetworkTranslationSync());
                }
                
                // this is to block this barrack to now allow other units to spawn...
                if (buildAction.ValueRO.time >= buildAction.ValueRO.duration)
                    ecb.RemoveComponent<BuildUnitAction>(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
