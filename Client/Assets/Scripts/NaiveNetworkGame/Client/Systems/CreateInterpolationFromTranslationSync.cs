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