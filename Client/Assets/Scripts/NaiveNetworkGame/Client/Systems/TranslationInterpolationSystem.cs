using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(CreateUnitFromNetworkGameStateSystem))]
    [UpdateBefore(typeof(UpdateUnitModelSystem))]
    public class TranslationInterpolationSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var localDeltaTime = Time.DeltaTime;
            
            // TODO: interpolate between real values...
            
            Entities
                .WithAll<Translation, TranslationInterpolation>()
                .ForEach(delegate(ref Translation t, ref TranslationInterpolation interpolation)
                {
                    interpolation.time += localDeltaTime;
                    
                    var t0 = interpolation.previousTranslation;
                    var t1 = interpolation.currentTranslation;
                    
                    interpolation.alpha = interpolation.time / interpolation.remoteDelta;

                    var t_current = math.lerp(t0, t1, math.clamp(interpolation.alpha, 0.0f, 1.0f));
                    
                    t.Value = new float3(t_current.x, t_current.y, 0);
                });
        }
    }
}