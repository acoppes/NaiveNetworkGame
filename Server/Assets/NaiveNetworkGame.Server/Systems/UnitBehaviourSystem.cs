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

            var targetsQuery = Entities.WithAll<Unit, Health, Translation, IsAlive>().ToEntityQuery();
            var targets = targetsQuery.ToEntityArray(Allocator.TempJob);
            var targetTranslations = targetsQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var targetUnits = targetsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);

            Entities
                .WithAll<Attack, Unit, Translation, IsAlive>()
                .WithNone<AttackTarget>()
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

            targets.Dispose();
            targetTranslations.Dispose();
            targetUnits.Dispose();
            
            Entities
                .WithAll<Attack, AttackTarget, IsAlive>()
                .WithNone<AttackAction, ReloadAction>()
                .ForEach(delegate(Entity e, ref Attack attack, ref AttackTarget target, ref Translation t)
                {
                    // if target entity was destroyed, forget about it...
                    if (!EntityManager.Exists(target.target))
                    {
                        PostUpdateCommands.RemoveComponent<AttackTarget>(e);
                    }
                    else
                    {
                        // if target too far away, forget about it...
                        var tp = EntityManager.GetComponentData<Translation>(target.target);
                        var isAlive = EntityManager.HasComponent<IsAlive>(target.target);
                        
                        if (!isAlive || math.distancesq(tp.Value, t.Value) > attack.range * attack.range)
                        {
                            PostUpdateCommands.RemoveComponent<AttackTarget>(e);
                        }
                        else
                        {
                            // if lost isalive, lose target...
                            PostUpdateCommands.AddComponent(e, new AttackAction());
                        }
                    } 
                });
            
            Entities
                .WithAll<Attack, AttackTarget, AttackAction, MovementAction>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.RemoveComponent<MovementAction>(e);
                });

            
            Entities
                .WithAll<Unit, Movement, UnitBehaviour, IsAlive>()
                .WithNone<ReloadAction>()
                .WithNone<MovementAction, SpawningAction, IdleAction, AttackAction, AttackTarget>()
                .ForEach(delegate (Entity e, ref UnitBehaviour behaviour)
                {
                    var offset = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, behaviour.range);
                    PostUpdateCommands.AddComponent(e, new MovementAction
                    {
                        target = behaviour.wanderCenter + new float2(offset.x, offset.y)
                    });
                    PostUpdateCommands.AddComponent(e, new IdleAction
                    {
                        time = UnityEngine.Random.Range(1.0f, 3.0f)
                    });
                });
        }
    }
}