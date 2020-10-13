using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    // [UpdateAfter(typeof(MovementActionSystem))]
    // public class DynamicObstacleSystem2 : ComponentSystem
    // {
    //     public struct DynamicObstaclesJob : IJob
    //     {
    //         public NativeArray<Translation> translations;
    //         public NativeArray<DynamicObstacle> obstacles;
    //         
    //         public void Execute()
    //         {
    //             // job1 (parallel)
    //             for (var i = 0; i < obstacles.Length; i++)
    //             {
    //                 var d = obstacles[i];
    //                 d.rangeSq = d.range * d.range;
    //                 obstacles[i] = d;
    //             }
    //             
    //             // job2 (parallel)
    //             for (var i = 0; i < translations.Length; i++)
    //             {
    //                 var t0 = translations[i];
    //                 var d0 = obstacles[i];
    //                 
    //                 for (var j = 0; j < translations.Length; j++)
    //                 {
    //                     if (i == j)
    //                         continue;
    //                     
    //                     var t1 = translations[j];
    //                     var d1 = obstacles[j];
    //
    //                     var m = t1.Value - t0.Value;
    //                     var d = math.lengthsq(m);
    //                     var r = d0.rangeSq + d1.rangeSq;
    //                     
    //                     if (d > r)
    //                         continue;
    //
    //                     var mlen = d - r;
    //                     d0.movement += math.normalizesafe(m, float3.zero) * mlen;
    //                 }
    //
    //                 obstacles[i] = d0;
    //             }
    //             
    //             // job3 (parallel)
    //             for (var i = 0; i < translations.Length; i++)
    //             {
    //                 var t = translations[i];
    //                 var d = obstacles[i];
    //
    //                 t.Value += d.movement;
    //                 d.movement = float3.zero;
    //
    //                 translations[i] = t;
    //                 obstacles[i] = d;
    //             }
    //         }
    //     }
    //
    //     private EntityQuery query;
    //
    //     protected override void OnCreate()
    //     {
    //         base.OnCreate();
    //         query = GetEntityQuery(ComponentType.ReadOnly<DynamicObstacle>(), 
    //             ComponentType.ReadWrite<Translation>());
    //     }
    //
    //     // protected override JobHandle OnUpdate(JobHandle inputDeps)
    //     // {
    //     //
    //     //     // tranlations.Dispose();
    //     //     // obstacles.Dispose();
    //     //
    //     //     return handle;
    //     // }
    //
    //     protected override void OnUpdate()
    //     {
    //         var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
    //         var obstacles = query.ToComponentDataArray<DynamicObstacle>(Allocator.TempJob);
    //         
    //         var job = new DynamicObstaclesJob
    //         {
    //             translations = translations,
    //             obstacles = obstacles
    //         };
    //         
    //         job.Schedule().Complete();
    //
    //         translations.Dispose();
    //         obstacles.Dispose();
    //     }
    // }

    [UpdateAfter(typeof(MovementActionSystem))]
    public class DynamicObstacleSystem : SystemBase
    {
        private EntityQuery obstaclesQuery;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            obstaclesQuery = GetEntityQuery(typeof(Translation), typeof(DynamicObstacle));
        }

        protected override void OnUpdate()
        {
            // var query = Entities.WithAll<Translation, DynamicObstacle>().ToEntityQuery();
            
            var translations = obstaclesQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var obstacles = obstaclesQuery.ToComponentDataArray<DynamicObstacle>(Allocator.TempJob);

            uint currentInternalIndex = 0;

            Entities
                .WithNone<IsAlive>()
                .WithAll<Health, DynamicObstacle>()
                .ForEach(delegate(ref DynamicObstacle d)
                {
                    // reduce priority if not alive but has health.
                    if (d.priority > 0)
                        d.priority = 1;
                }).ScheduleParallel();
            
            Entities
                .WithAll<DynamicObstacle>()
                .ForEach(delegate(ref DynamicObstacle d)
                {
                    d.rangeSq = d.range * d.range;
                    d.index = currentInternalIndex++;
                }).Run();

            Entities
                .WithAll<Translation, DynamicObstacle>()
                .ForEach(delegate(Entity e, ref DynamicObstacle d0, in Translation t0)
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
                .ForEach(delegate(ref Translation t, ref DynamicObstacle d)
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