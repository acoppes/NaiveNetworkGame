using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class TargetingSystem : ComponentSystem
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
                .WithAll<AttackComponent, Unit, Translation, IsAlive>()
                .WithNone<AttackTargetComponent, SpawningAction, DeathAction, ReloadAction, DisableAttackComponent>()
                .ForEach(delegate(Entity e, ref AttackComponent attack, ref Unit unit, ref Translation t)
                {
                    // search for targets near my range...
                    for (var i = 0; i < targetUnits.Length; i++)
                    {
                        // dont attack my units
                        if (targetUnits[i].player == unit.player)
                            continue;

                        if (math.distancesq(t.Value, targetTranslations[i].Value) < attack.range * attack.range)
                        {
                            PostUpdateCommands.AddComponent(e, new AttackTargetComponent
                            {
                                target = targets[i]
                            });
                            return;
                        }
                    }
                });
            
            // TODO: search for best target here (distance, less targeting units, etc)
            Entities
                .WithAll<AttackComponent, Unit, IsAlive>()
                .WithNone<AttackTargetComponent, ChaseTargetComponent, SpawningAction, DeathAction, ReloadAction>()
                .WithNone<DisableAttackComponent>()
                .ForEach(delegate(Entity e, ref AttackComponent attack, ref Unit unit)
                {
                    // var bestTarget = Entity.Null;
                    var bestTargetIndex = -1;
                    var bestTargetDistanceSq = float.MaxValue;
                    var chaseRangeSq = attack.chaseRange * attack.chaseRange;

                    var chaseCenter = attack.chaseCenter;
                    
                    // search for targets near my range...
                    for (var i = 0; i < targetUnits.Length; i++)
                    {
                        // dont attack my units
                        if (targetUnits[i].player == unit.player)
                            continue;

                        // inside chase area
                        var currentDistanceSq = math.distancesq(chaseCenter, targetTranslations[i].Value);
                        if (currentDistanceSq < chaseRangeSq && currentDistanceSq < bestTargetDistanceSq)
                        {
                            bestTargetIndex = i;
                            bestTargetDistanceSq = currentDistanceSq;
                        }
                    }
                    
                    if (bestTargetIndex == -1)
                        return;

                    PostUpdateCommands.AddComponent(e, new ChaseTargetComponent
                    {
                        target = targets[bestTargetIndex]
                    });
                });

            targets.Dispose();
            targetTranslations.Dispose();
            targetUnits.Dispose();

        }
    }
}