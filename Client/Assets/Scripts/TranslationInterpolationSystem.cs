using Client;
using Mockups;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Scenes
{
    public struct TranslationInterpolation : IComponentData
    {
        // public int currentFrame;
        // public int nextFrame;
        
        public float time;
        public float alpha;

        public float2 previousTranslation;
        public float2 currentTranslation;

        public float localDelta;
        public float remoteDelta;
    }
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(ClientViewSystem))]
    [UpdateBefore(typeof(VisualModelUpdatePositionSystem))]
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
                    interpolation.localDelta = localDeltaTime;
                    
                    var t0 = interpolation.previousTranslation;
                    var t1 = interpolation.currentTranslation;
                    
                    interpolation.alpha = interpolation.time / interpolation.remoteDelta;

                    var t_current = math.lerp(t0, t1, math.clamp(interpolation.alpha, 0.0f, 1.0f));
                    
                    t.Value = new float3(t_current.x, t_current.y, 0);
                });
            
            // Entities
            //     .WithAll<LookingDirection, TranslationInterpolation>()
            //     .ForEach(delegate(ref LookingDirection l, ref TranslationInterpolation interpolation)
            //     {
            //         var distance = math.distancesq(interpolation.previousTranslation, interpolation.currentTranslation);
            //         if (distance > 0.01f)
            //             l.direction = interpolation.previousTranslation - interpolation.currentTranslation;
            //     });
        }
    }
}