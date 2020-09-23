using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public class DestroyDeathUnitsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Health>()
                .ForEach(delegate(Entity e, ref Health h)
                {
                    // TODO: change to death action

                    if (h.current <= 0.01f)
                    {
                        PostUpdateCommands.DestroyEntity(e);
                    }
                });
        }
    }
}