using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(CreateUnitFromNetworkGameStateSystem))]
    [UpdateBefore(typeof(UpdateUnitModelSystem))]
    public partial struct TranslationInterpolationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var localDeltaTime = SystemAPI.Time.DeltaTime;
            
            // TODO: interpolate between real values...
            
            foreach (var (localTransform, interpolation) in 
                SystemAPI.Query<RefRW<LocalTransform>, RefRW<TranslationInterpolation>>())
            {
                interpolation.ValueRW.time += localDeltaTime;
                
                var t0 = interpolation.ValueRO.previousTranslation;
                var t1 = interpolation.ValueRO.currentTranslation;
                
                interpolation.ValueRW.alpha = interpolation.ValueRO.time / interpolation.ValueRO.remoteDelta;

                var t_current = math.lerp(t0, t1, math.clamp(interpolation.ValueRO.alpha, 0.0f, 1.0f));
                
                localTransform.ValueRW.Position = new float3(t_current.x, t_current.y, 0);
            }
        }
    }
}
