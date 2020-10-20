using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ServerProcessPendingPlayerActionsSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class ProcessBuildUnitAction : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<BuildUnitAction, BuildingHolder>()
                .ForEach(delegate(Entity entity, ref BuildingHolder holder)
                {
                    holder.hasBuilding = true;
                });
            
            Entities
                .WithAll<ServerOnly, BuildUnitAction, Unit>()
                .ForEach(delegate(Entity e, ref BuildUnitAction buildAction, ref UnitSpawnPosition spawnPosition, ref Unit barrackUnit, 
                    ref Translation t)
                {
                    buildAction.time += Time.DeltaTime;

                    if (!buildAction.unitSpawning)
                    {
                        buildAction.unitSpawning = true;
                        
                        var unitComponent = EntityManager.GetComponentData<Unit>(buildAction.prefab);
                        buildAction.duration = unitComponent.spawnDuration;

                        // delay? 
                        var unitEntity = PostUpdateCommands.Instantiate(buildAction.prefab);
                    
                        unitComponent.id = NetworkUnitId.current++;
                        unitComponent.player = barrackUnit.player;
                    
                        PostUpdateCommands.SetComponent(unitEntity, unitComponent);
                    
                        PostUpdateCommands.SetComponent(unitEntity, new Translation
                        {
                            Value = t.Value + spawnPosition.position
                        });
                    
                        PostUpdateCommands.SetComponent(unitEntity, new UnitState
                        {
                            state = UnitState.spawningState
                        });
                    
                        PostUpdateCommands.AddComponent(unitEntity, new SpawningAction
                        {
                            duration =  buildAction.duration 
                        });

                        var wanderArea = buildAction.wanderArea;

                        PostUpdateCommands.AddComponent(unitEntity, new UnitBehaviour
                        {
                            wanderArea = wanderArea,
                            minIdleTime = 1,
                            maxIdleTime = 3
                        });
                    
                        PostUpdateCommands.AddComponent<NetworkUnit>(unitEntity);
                        PostUpdateCommands.AddComponent(unitEntity, new NetworkGameState());
                        PostUpdateCommands.AddComponent(unitEntity, new NetworkTranslationSync());
                    }
                    
                    // this is to block this barrack to now allow other units to spawn...
                    if (buildAction.time >= buildAction.duration)
                        PostUpdateCommands.RemoveComponent<BuildUnitAction>(e);
                });
        }
    }
}