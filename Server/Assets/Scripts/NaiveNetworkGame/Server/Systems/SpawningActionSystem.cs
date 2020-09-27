using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    // Processes spawn over time logic for units...
    
    [UpdateBefore(typeof(UnitStateSystem))]
    public class SpawningActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities
                .WithAll<SpawningAction>()
                .ForEach(delegate(Entity e, ref SpawningAction s)
                {
                    s.time += dt;

                    if (s.time >= s.duration)
                    {
                        PostUpdateCommands.RemoveComponent<SpawningAction>(e);
                    }
                });
        }
    }
}