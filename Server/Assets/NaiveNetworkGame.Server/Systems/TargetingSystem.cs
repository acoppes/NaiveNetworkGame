using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NaiveNetworkGame.Server.Systems
{
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
            
            // TODO: search for best target here (distance, less targeting units, etc)
            Entities
                .WithAll<Attack, Unit, Translation, IsAlive>()
                .WithNone<AttackTarget, ChaseTarget, SpawningAction, DeathAction, ReloadAction>()
                .ForEach(delegate(Entity e, ref Attack attack, ref Unit unit, ref Translation t)
                {
                    // var bestTarget = Entity.Null;
                    var bestTargetIndex = -1;
                    var bestTargetDistanceSq = float.MaxValue;
                    var chaseRangeSq = attack.chaseRange * attack.chaseRange;
                    
                    // search for targets near my range...
                    for (var i = 0; i < targetUnits.Length; i++)
                    {
                        // dont attack my units
                        if (targetUnits[i].player == unit.player)
                            continue;

                        // inside chase area
                        var currentDistanceSq = math.distancesq(t.Value, targetTranslations[i].Value);
                        if (currentDistanceSq < chaseRangeSq && currentDistanceSq < bestTargetDistanceSq)
                        {
                            bestTargetIndex = i;
                            bestTargetDistanceSq = currentDistanceSq;
                        }
                    }
                    
                    if (bestTargetIndex == -1)
                        return;

                    PostUpdateCommands.AddComponent(e, new ChaseTarget
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