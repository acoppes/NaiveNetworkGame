using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct TargetingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // for each unit, calculate best target
            // order by number of attacking units and select it

            // if unit has attack target and not spawning, idle, etc
            // move to attack target...

            // if unit is in attack position
            // perform attack, wait, attack, wait...
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var targetsQuery = SystemAPI.QueryBuilder()
                .WithNone<SpawningAction, DeathAction>()
                .WithAll<Unit, Health, LocalTransform, IsAlive>()
                .Build();
            
            var targets = targetsQuery.ToEntityArray(Allocator.TempJob);
            var targetTransforms = targetsQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var targetUnits = targetsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);

            foreach (var (attack, unit, localTransform, entity) in 
                SystemAPI.Query<RefRO<AttackComponent>, RefRO<Unit>, RefRO<LocalTransform>>()
                    .WithNone<AttackTargetComponent, SpawningAction, DeathAction>()
                    .WithNone<ReloadAction, DisableAttackComponent>()
                    .WithAll<IsAlive>()
                    .WithEntityAccess())
            {
                // search for targets near my range...
                for (var i = 0; i < targetUnits.Length; i++)
                {
                    // dont attack my units
                    if (targetUnits[i].player == unit.ValueRO.player)
                        continue;

                    if (math.distancesq(localTransform.ValueRO.Position, targetTransforms[i].Position) < attack.ValueRO.range * attack.ValueRO.range)
                    {
                        ecb.AddComponent(entity, new AttackTargetComponent
                        {
                            target = targets[i]
                        });
                        goto NextEntity;
                    }
                }
                NextEntity:;
            }
            
            // TODO: search for best target here (distance, less targeting units, etc)
            foreach (var (attack, unit, entity) in 
                SystemAPI.Query<RefRO<AttackComponent>, RefRO<Unit>>()
                    .WithNone<AttackTargetComponent, ChaseTargetComponent, SpawningAction>()
                    .WithNone<DeathAction, ReloadAction, DisableAttackComponent>()
                    .WithAll<IsAlive>()
                    .WithEntityAccess())
            {
                var bestTargetIndex = -1;
                var bestTargetDistanceSq = float.MaxValue;
                var chaseRangeSq = attack.ValueRO.chaseRange * attack.ValueRO.chaseRange;

                var chaseCenter = attack.ValueRO.chaseCenter;
                
                // search for targets near my range...
                for (var i = 0; i < targetUnits.Length; i++)
                {
                    // dont attack my units
                    if (targetUnits[i].player == unit.ValueRO.player)
                        continue;

                    // inside chase area
                    var currentDistanceSq = math.distancesq(chaseCenter, targetTransforms[i].Position);
                    if (currentDistanceSq < chaseRangeSq && currentDistanceSq < bestTargetDistanceSq)
                    {
                        bestTargetIndex = i;
                        bestTargetDistanceSq = currentDistanceSq;
                    }
                }
                
                if (bestTargetIndex == -1)
                    continue;

                ecb.AddComponent(entity, new ChaseTargetComponent
                {
                    target = targets[bestTargetIndex]
                });
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            targets.Dispose();
            targetTransforms.Dispose();
            targetUnits.Dispose();
        }
    }
}
