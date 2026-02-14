using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(MovementActionSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct DynamicObstacleSystem : ISystem
    {
        private EntityQuery obstaclesQuery;
        
        public void OnCreate(ref SystemState state)
        {
            obstaclesQuery = state.GetEntityQuery(typeof(LocalTransform), typeof(DynamicObstacle));
        }
        
        public void OnUpdate(ref SystemState state)
        {
            // var query = Entities.WithAll<Translation, DynamicObstacle>().ToEntityQuery();
            
            var translations = obstaclesQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var obstacles = obstaclesQuery.ToComponentDataArray<DynamicObstacle>(Allocator.TempJob);

            uint currentInternalIndex = 0;
            
            foreach (var d in SystemAPI
                         .Query<RefRW<DynamicObstacle>>()
                         .WithAll<Health>()
                         .WithNone<IsAlive>())
            {
                if (d.ValueRW.priority > 0)
                    d.ValueRW.priority = 1;
            }
            
            // Entities
            //     .WithNone<IsAlive>()
            //     .WithAll<Health, DynamicObstacle>()
            //     .ForEach((ref DynamicObstacle d) =>
            //     {
            //         // reduce priority if not alive but has health.
            //         if (d.priority > 0)
            //             d.priority = 1;
            //     }).ScheduleParallel();
            
            foreach (var d in SystemAPI
                         .Query<RefRW<DynamicObstacle>>())
            {
                d.ValueRW.rangeSq = d.ValueRW.range * d.ValueRW.range;
                d.ValueRW.index = currentInternalIndex++;
            }
            
            // Entities
            //     .WithAll<DynamicObstacle>()
            //     .ForEach((ref DynamicObstacle d) =>
            //     {
            //         d.rangeSq = d.range * d.range;
            //         d.index = currentInternalIndex++;
            //     }).Run();
            
            foreach (var (d0RW, t0RO) in SystemAPI
                         .Query<RefRW<DynamicObstacle>, RefRO<LocalTransform>>())
            {
                ref var dynamicObstacle = ref d0RW.ValueRW;
                var localTransform = t0RO.ValueRO;
                
                if (dynamicObstacle.priority == 0)
                    continue;

                for (var j = 0; j < translations.Length; j++)
                {
                    var d1 = obstacles[j];

                    if (dynamicObstacle.index == d1.index)
                        continue;

                    if (dynamicObstacle.priority > d1.priority)
                        continue;

                    var t1 = translations[j];

                    var m = t1.Position - localTransform.Position;
                    var d = math.lengthsq(m);
                    var r = dynamicObstacle.rangeSq + d1.rangeSq;

                    if (d > r)
                        continue;

                    // if both of the units are moving, then only move half the distance.
                    var mult = 0.5f;

                    if (dynamicObstacle.priority < d1.priority)
                        mult = 1.0f;

                    var mlen = (d - r) * mult;
                    dynamicObstacle.movement += math.normalizesafe(m, float3.zero) * mlen;
                }
            }

            // Entities
            //     .WithAll<LocalTransform, DynamicObstacle>()
            //     .ForEach((Entity e, ref DynamicObstacle d0, in LocalTransform t0) =>
            //     {
            //         // we have the logic disabled...
            //         if (d0.priority == 0)
            //             return;
            //
            //         for (var j = 0; j < translations.Length; j++)
            //         {
            //             var d1 = obstacles[j];
            //
            //             if (d0.index == d1.index)
            //                 continue;
            //
            //             if (d0.priority > d1.priority)
            //                 continue;
            //
            //             var t1 = translations[j];
            //
            //             var m = t1.Position - t0.Position;
            //             var d = math.lengthsq(m);
            //             var r = d0.rangeSq + d1.rangeSq;
            //
            //             if (d > r)
            //                 continue;
            //
            //             // if both of the units are moving, then only move half the distance.
            //             var mult = 0.5f;
            //
            //             if (d0.priority < d1.priority)
            //                 mult = 1.0f;
            //
            //             var mlen = (d - r) * mult;
            //             d0.movement += math.normalizesafe(m, float3.zero) * mlen;
            //         }
            //     }).Run();
            
            foreach (var (d, t) in SystemAPI
                         .Query<RefRW<DynamicObstacle>, RefRW<LocalTransform>>())
            {
                t.ValueRW.Position += d.ValueRW.movement;
                d.ValueRW.movement = float3.zero;
            }
            
            // Entities
            //     .WithAll<LocalTransform, DynamicObstacle>()
            //     .ForEach((ref LocalTransform t, ref DynamicObstacle d) =>
            //     {
            //         t.Position += d.movement;
            //         d.movement = float3.zero;
            //     }).ScheduleParallel();
            
            // entities.Dispose();
            obstacles.Dispose();
            translations.Dispose();
        }
    }
}