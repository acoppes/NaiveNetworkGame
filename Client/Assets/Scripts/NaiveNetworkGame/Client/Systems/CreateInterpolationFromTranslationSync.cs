// using NaiveNetworkGame.Common;
// using Unity.Entities;
// using Unity.Transforms;
//
// namespace NaiveNetworkGame.Client.Systems
// {
//     [UpdateAfter(typeof(CopyTranslationSyncToUnit))]
//     public partial class CreateInterpolationFromTranslationSync : SystemBase
//     {
//         protected override void OnUpdate()
//         {
//             // TODO: we might add the translation component too here...
//             
//             // first time interpolation created with proper translation...
//             Entities
//                 .WithNone<TranslationInterpolation>()
//                 .WithAll<Unit, Translation, NetworkTranslationSync, ClientOnly>()
//                 .ForEach(delegate(Entity e, ref Translation t, ref NetworkTranslationSync n)
//                 {
//                     // interpolation component was created with unit the first time...
//                     
//                     PostUpdateCommands.AddComponent(e, new TranslationInterpolation
//                     {
//                         previousTranslation = n.translation,
//                         currentTranslation = n.translation,
//                         time = 0,
//                         remoteDelta = n.delta
//                     });
//                 });
//             
//             Entities
//                 .WithAll<Unit, Translation, NetworkTranslationSync, ClientOnly>()
//                 .ForEach(delegate(Entity e, ref Translation t, ref NetworkTranslationSync n,
//                     ref TranslationInterpolation interpolation)
//                 {
//                     // interpolation component was created with unit the first time...
//             
//                     interpolation.previousTranslation = t.Value.xy;
//                     interpolation.currentTranslation = n.translation;
//                     interpolation.remoteDelta = n.delta;
//                     interpolation.time = 0;
//                     
//                     PostUpdateCommands.RemoveComponent<NetworkTranslationSync>(e);
//                 });
//         }
//     }
// }

using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateAfter(typeof(CopyTranslationSyncToUnit))]
    public partial struct CreateInterpolationFromTranslationSync : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // First time interpolation created with proper translation...
            foreach (var (_, networkSync, entity) in 
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<NetworkTranslationSync>>()
                         .WithNone<TranslationInterpolation>()
                         .WithAll<Unit, ClientOnly>()
                         .WithEntityAccess())
            {
                state.EntityManager.AddComponentData(entity, new TranslationInterpolation
                {
                    previousTranslation = networkSync.ValueRO.translation,
                    currentTranslation = networkSync.ValueRO.translation,
                    time = 0,
                    remoteDelta = networkSync.ValueRO.delta
                });
            }
            
            // Update existing interpolation components
            foreach (var (localTransform, networkSync, interpolationRW, entity) in 
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<NetworkTranslationSync>, RefRW<TranslationInterpolation>>()
                         .WithAll<Unit, ClientOnly>()
                         .WithEntityAccess())
            {
                ref var interpolation = ref interpolationRW.ValueRW;
                
                interpolation.previousTranslation = localTransform.ValueRO.Position.xy;
                interpolation.currentTranslation = networkSync.ValueRO.translation;
                interpolation.remoteDelta = networkSync.ValueRO.delta;
                interpolation.time = 0;
                
                state.EntityManager.RemoveComponent<NetworkTranslationSync>(entity);
            }
        }
    }
}