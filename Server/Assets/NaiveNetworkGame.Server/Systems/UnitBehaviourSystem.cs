using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    // Just a small default wander (around 0,0) behaviour for units...

    [UpdateAfter(typeof(TargetingSystem))]
    public class UnitBehaviourSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // if we have attack target, remove chase target...
            Entities
                .WithAll<AttackTarget, ChaseTarget>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.RemoveComponent<ChaseTarget>(e);
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
                .WithAll<ChaseTarget>()
                .ForEach(delegate(Entity e, ref ChaseTarget chaseTarget)
                {
                    if (!EntityManager.Exists(chaseTarget.target))
                    {
                        PostUpdateCommands.RemoveComponent<ChaseTarget>(e);
                    }
                    else
                    {
                        var isAlive = EntityManager.HasComponent<IsAlive>(chaseTarget.target);
                        if (!isAlive)
                        {
                            PostUpdateCommands.RemoveComponent<ChaseTarget>(e);
                        }
                    } 
                });
        

            Entities
                .WithAll<Unit, Movement, UnitBehaviour, IsAlive, ChaseTarget>()
                .WithNone<ReloadAction, DeathAction>()
                .WithNone<MovementAction, SpawningAction, AttackAction, AttackTarget>()
                .ForEach(delegate (Entity e, ref ChaseTarget chaseTarget)
                {
                    var t = GetComponentDataFromEntity<Translation>()[chaseTarget.target];
                    
                    PostUpdateCommands.AddComponent(e, new MovementAction
                    {
                        target = t.Value.xy
                    });
                });
            
            Entities
                .WithAll<Unit, Movement, UnitBehaviour, IsAlive, ChaseTarget>()
                .WithAll<MovementAction>()
                .WithNone<ReloadAction, DeathAction>()
                .WithNone<SpawningAction, AttackAction, AttackTarget>()
                .ForEach(delegate (Entity e, ref MovementAction m, ref ChaseTarget chaseTarget)
                {
                    var t = GetComponentDataFromEntity<Translation>()[chaseTarget.target];
                    m.target = t.Value.xy;
                });
            
            Entities
                .WithAll<Unit, Movement, UnitBehaviour, IsAlive>()
                .WithNone<ReloadAction, ChaseTarget, DeathAction>()
                .WithNone<MovementAction, SpawningAction, IdleAction, AttackAction, AttackTarget>()
                .ForEach(delegate (Entity e, ref UnitBehaviour behaviour)
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