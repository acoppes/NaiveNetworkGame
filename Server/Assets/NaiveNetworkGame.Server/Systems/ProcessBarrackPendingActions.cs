using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ServerProcessPendingPlayerActionsSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class ProcessBarrackPendingActions : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // TODO: delegate the build structure to holder entities? 
            
            Entities
                .WithAll<ServerOnly, BuildUnitAction, Barracks, Unit, Skin>()
                .ForEach(delegate(Entity e, ref BuildUnitAction buildAction, ref Barracks b, ref Unit barrackUnit, ref Skin s)
                {
                    buildAction.time += Time.DeltaTime;

                    if (buildAction.time < buildAction.duration)
                        return;
                    
                    PostUpdateCommands.RemoveComponent<BuildUnitAction>(e);

                    var unitComponent = EntityManager.GetComponentData<Unit>(buildAction.prefab);

                    // delay? 
                    var unitEntity = PostUpdateCommands.Instantiate(buildAction.prefab);
                    
                    unitComponent.id = NetworkUnitId.current++;
                    unitComponent.player = barrackUnit.player;
                    
                    PostUpdateCommands.SetComponent(unitEntity, unitComponent);
                    
                    PostUpdateCommands.AddComponent(unitEntity, s);
                    PostUpdateCommands.SetComponent(unitEntity, new Translation
                    {
                        Value = b.spawnPosition
                    });
                    
                    PostUpdateCommands.SetComponent(unitEntity, new UnitState
                    {
                        state = UnitState.spawningState
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
                });
        }
    }
}