using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Client.Systems
{
    public partial struct CopyTranslationSyncToUnit : ISystem
    {
        private EntityQuery unitsQuery;

        public void OnCreate(ref SystemState state)
        {
            unitsQuery = state.GetEntityQuery(typeof(Unit), typeof(LocalTransform));
        }

        public void OnUpdate(ref SystemState state)
        {
            // var unitsQuery = Entities.WithAll<Unit, LocalTransform>().ToQuery();
            
            var units = unitsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);
            var unitEntities = unitsQuery.ToEntityArray(Allocator.TempJob);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (n, e) in SystemAPI.Query<NetworkTranslationSync>()
                         .WithAll<NetworkTranslationSync, ClientOnly>()
                         .WithNone<Unit>()
                         .WithEntityAccess())
            {
                for (var i = 0; i < units.Length; i++)
                {
                    var unit = units[i];
                    if (unit.unitId == n.unitId)
                    {
                        ecb.AddComponent(unitEntities[i], n);
                    }
                }

                ecb.DestroyEntity(e);
            }
            
            // Entities
            //     .WithNone<Unit>()
            //     .WithAll<NetworkTranslationSync, ClientOnly>()
            //     .ForEach((Entity e, ref NetworkTranslationSync n) =>
            //     {
            //         for (var i = 0; i < units.Length; i++)
            //         {
            //             var unit = units[i];
            //             if (unit.unitId == n.unitId)
            //             {
            //                 PostUpdateCommands.AddComponent(unitEntities[i], n);
            //             }
            //         }
            //
            //         PostUpdateCommands.DestroyEntity(e);
            //     }).Run();
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            units.Dispose();
            unitEntities.Dispose();
        }
    }
}