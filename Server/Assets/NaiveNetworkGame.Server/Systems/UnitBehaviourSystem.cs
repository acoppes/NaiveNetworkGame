using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    // Just a small default wander (around 0,0) behaviour for units...

    public class UnitBehaviourSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // for each unit, calculate best target
            // order by number of attacking units and select it

            // if unit has attack target and not spawning, idle, etc
            // move to attack target...

            // if unit is in attack position
            // perform attack, wait, attack, wait...

            var targetsQuery = Entities
                .WithNone<SpawningAction, DeathAction>()
                .WithAll<Unit, Health, Translation, IsAlive>()
                .ToEntityQuery();
            
            var targets = targetsQuery.ToEntityArray(Allocator.TempJob);
            var targetTranslations = targetsQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var targetUnits = targetsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);

            Entities
                .WithAll<Attack, Unit, Translation, IsAlive>()
                .WithNone<AttackTarget, SpawningAction, DeathAction, ReloadAction>()
                .ForEach(delegate(Entity e, ref Attack attack, ref Unit unit, ref Translation t)
                {
                    // search for targets near my range...
                    for (var i = 0; i < targetUnits.Length; i++)
                    {
                        // dont attack my units
                        if (targetUnits[i].player == unit.player)
                            continue;

                        if (math.distancesq(t.Value, targetTranslations[i].Value) < attack.range * attack.range)
                        {
                            PostUpdateCommands.AddComponent(e, new AttackTarget
                            {
                                target = targets[i]
                            });
                            return;
                        }
                    }
                });
            
            // if we have attack target, remove chase target...
            Entities
                .WithAll<AttackTarget, ChaseTarget>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.RemoveComponent<ChaseTarget>(e);
                });

            // TODO: search for best target here (distance, less targeting units, etc)
            Entities
                .WithAll<Attack, Unit, Translation, IsAlive>()
                .WithNone<AttackTarget, ChaseTarget, SpawningAction, DeathAction, ReloadAction>()
                .ForEach(delegate(Entity e, ref Attack attack, ref Unit unit, ref Translation t)
                {
                    // search for targets near my range...
                    for (var i = 0; i < targetUnits.Length; i++)
                    {
                        // dont attack my units
                        if (targetUnits[i].player == unit.player)
                            continue;

                        if (math.distancesq(t.Value, targetTranslations[i].Value) < attack.chaseRange * attack.chaseRange)
                        {
                            PostUpdateCommands.AddComponent(e, new ChaseTarget
                            {
                                target = targets[i]
                            });
                            return;
                        }
                    }
                });

            targets.Dispose();
            targetTranslations.Dispose();
            targetUnits.Dispose();

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