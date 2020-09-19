using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateAfter(typeof(CopyTranslationSyncToUnit))]
    public class CreateInterpolationFromTranslationSync : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // TODO: we might add the translation component too here...
            
            // first time interpolation created with proper translation...
            Entities
                .WithNone<TranslationInterpolation>()
                .WithAll<Unit, Translation, NetworkTranslationSync>()
                .ForEach(delegate(Entity e, ref Translation t, ref NetworkTranslationSync n)
                {
                    // interpolation component was created with unit the first time...
                    
                    PostUpdateCommands.AddComponent(e, new TranslationInterpolation
                    {
                        previousTranslation = n.translation,
                        currentTranslation = n.translation,
                        time = 0,
                        remoteDelta = n.delta
                    });
                });
            
            Entities
                .WithAll<Unit, Translation, NetworkTranslationSync>()
                .ForEach(delegate(Entity e, ref Translation t, ref NetworkTranslationSync n,
                    ref TranslationInterpolation interpolation)
                {
                    // interpolation component was created with unit the first time...
            
                    interpolation.previousTranslation = t.Value.xy;
                    interpolation.currentTranslation = n.translation;
                    interpolation.remoteDelta = n.delta;
                    interpolation.time = 0;
                    
                    PostUpdateCommands.RemoveComponent<NetworkTranslationSync>(e);
                });
        }
    }
}