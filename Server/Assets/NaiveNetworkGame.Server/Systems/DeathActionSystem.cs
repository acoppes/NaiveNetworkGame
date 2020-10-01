using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public class DeathActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities
                .WithNone<IsAlive>()
                .WithAll<DeathAction>()
                .ForEach(delegate(Entity e, ref DeathAction a)
                {
                    a.time += dt;
                    if (a.time > a.duration)
                    {
                        // set completely death?
                        PostUpdateCommands.RemoveComponent<DeathAction>(e);
                        PostUpdateCommands.RemoveComponent<NetworkTranslationSync>(e);
                        PostUpdateCommands.RemoveComponent<NetworkGameState>(e);
                    }
                });
        }
    }
}