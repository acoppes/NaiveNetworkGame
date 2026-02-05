using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(TargetingSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class UnitBehaviourSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // if we have attack target, remove chase target...
            Entities
                .WithAll<AttackTargetComponent, ChaseTargetComponent>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.RemoveComponent<ChaseTargetComponent>(e);
                });
            
            
            // Stop chasing if target is not valid or is not alive
            // Entities
            //     .WithAll<ChaseAction>()
            //     .ForEach(delegate(Entity e, ref ChaseAction chasingAction)
            //     {
            //         if (!EntityManager.Exists(chasingAction.target))
            //         {
            //             PostUpdateCommands.RemoveComponent<ChaseAction>(e);
            //         }
            //         else
            //         {
            //             var isAlive = EntityManager.HasComponent<IsAlive>(chasingAction.target);
            //             if (!isAlive)
            //             {
            //                 PostUpdateCommands.RemoveComponent<ChaseAction>(e);
            //             }
            //         } 
            //     });
            
            Entities
                .WithAll<ChaseTargetComponent>()
                .ForEach(delegate(Entity e, ref ChaseTargetComponent chaseTarget)
                {
                    if (!EntityManager.Exists(chaseTarget.target))
                    {
                        PostUpdateCommands.RemoveComponent<ChaseTargetComponent>(e);
                    }
                    else
                    {
                        var isAlive = EntityManager.HasComponent<IsAlive>(chaseTarget.target);
                        if (!isAlive)
                        {
                            PostUpdateCommands.RemoveComponent<ChaseTargetComponent>(e);
                        }
                    } 
                });
        

            Entities
                .WithAll<Unit, Movement, UnitBehaviourComponent, IsAlive, ChaseTargetComponent>()
                .WithNone<ReloadAction, DeathAction>()
                .WithNone<MovementAction, SpawningAction, AttackAction, AttackTargetComponent>()
                .ForEach(delegate (Entity e, ref ChaseTargetComponent chaseTarget)
                {
                    var t = GetComponentDataFromEntity<Translation>()[chaseTarget.target];
                    
                    PostUpdateCommands.AddComponent(e, new MovementAction
                    {
                        target = t.Value.xy
                    });
                });
            
            Entities
                .WithAll<Unit, Movement, UnitBehaviourComponent, IsAlive, ChaseTargetComponent>()
                .WithAll<MovementAction>()
                .WithNone<ReloadAction, DeathAction>()
                .WithNone<SpawningAction, AttackAction, AttackTargetComponent>()
                .ForEach(delegate (Entity e, ref MovementAction m, ref ChaseTargetComponent chaseTarget)
                {
                    var t = GetComponentDataFromEntity<Translation>()[chaseTarget.target];
                    m.target = t.Value.xy;
                });
            
            Entities
                .WithAll<Unit, Movement, UnitBehaviourComponent, IsAlive>()
                .WithNone<ReloadAction, ChaseTargetComponent, DeathAction>()
                .WithNone<MovementAction, SpawningAction, IdleAction, AttackAction, AttackTargetComponent>()
                .ForEach(delegate (Entity e, ref UnitBehaviourComponent behaviour)
                {
                    var wanderAreaEntity = behaviour.wanderArea;
                    
                    if (EntityManager.Exists(wanderAreaEntity))
                    {
                        var wanderArea = GetComponentDataFromEntity<WanderArea>()[wanderAreaEntity];
                        var wanderCenter = GetComponentDataFromEntity<Translation>()[wanderAreaEntity];
                        
                        var offset = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, wanderArea.range);
                        PostUpdateCommands.AddComponent(e, new MovementAction
                        {
                            target = wanderCenter.Value.xy + new float2(offset.x, offset.y)
                        });
                        PostUpdateCommands.AddComponent(e, new IdleAction
                        {
                            time = UnityEngine.Random.Range(behaviour.minIdleTime, behaviour.maxIdleTime)
                        });    
                    }
                });
        }
    }
}