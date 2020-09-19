using Client;
using NaiveNetworkGame.Common;
using Scenes;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateAfter(typeof(CopyTranslationSyncToUnit))]
    public class CreateInterpolationFromTranslationSync : ComponentSystem
    {
        protected override void OnUpdate()
        {
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