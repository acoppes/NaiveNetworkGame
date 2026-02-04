using NaiveNetworkGame.Server.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(MovementActionSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    [BurstCompile]
    public partial class DynamicObstacleSystem : SystemBase
    {
        private EntityQuery obstaclesQuery;
        
        [BurstCompile]
        protected override void OnCreate()
        {
            base.OnCreate();
            obstaclesQuery = GetEntityQuery(typeof(Translation), typeof(DynamicObstacle));
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            // var query = Entities.WithAll<Translation, DynamicObstacle>().ToEntityQuery();
            
            var translations = obstaclesQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var obstacles = obstaclesQuery.ToComponentDataArray<DynamicObstacle>(Allocator.TempJob);

            uint currentInternalIndex = 0;

            Entities
                .WithNone<IsAlive>()
                .WithAll<Health, DynamicObstacle>()
                .ForEach((ref DynamicObstacle d) =>
                {
                    // reduce priority if not alive but has health.
                    if (d.priority > 0)
                        d.priority = 1;
                }).ScheduleParallel();
            
            Entities
                .WithAll<DynamicObstacle>()
                .ForEach((ref DynamicObstacle d) =>
                {
                    d.rangeSq = d.range * d.range;
                    d.index = currentInternalIndex++;
                }).Run();

            Entities
                .WithAll<Translation, DynamicObstacle>()
                .ForEach((Entity e, ref DynamicObstacle d0, in Translation t0) =>
                {
                    // we have the logic disabled...
                    if (d0.priority == 0)
                        return;

                    for (var j = 0; j < translations.Length; j++)
                    {
                        var d1 = obstacles[j];

                        if (d0.index == d1.index)
                            continue;

                        if (d0.priority > d1.priority)
                            continue;

                        var t1 = translations[j];

                        var m = t1.Value - t0.Value;
                        var d = math.lengthsq(m);
                        var r = d0.rangeSq + d1.rangeSq;

                        if (d > r)
                            continue;

                        // if both of the units are moving, then only move half the distance.
                        var mult = 0.5f;

                        if (d0.priority < d1.priority)
                            mult = 1.0f;

                        var mlen = (d - r) * mult;
                        d0.movement += math.normalizesafe(m, float3.zero) * mlen;
                    }
                }).Run();
            
            Entities
                .WithAll<Translation, DynamicObstacle>()
                .ForEach((ref Translation t, ref DynamicObstacle d) =>
                {
                    t.Value += d.movement;
                    d.movement = float3.zero;
                }).ScheduleParallel();
            
            // entities.Dispose();
            obstacles.Dispose();
            translations.Dispose();
        }
    }
}