using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public class DeathOnNoHealthUnitSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Health, IsAlive, UnitBehaviour>()
                .ForEach(delegate(Entity e, ref Health h)
                {
                    if (h.current <= 0.01f)
                    {
                        // TODO: configure death action with unit behaviour or unit data, or new component death
                        PostUpdateCommands.AddComponent(e, new DeathAction
                        {
                            time = 0, 
                            duration = 1
                        });
                        PostUpdateCommands.RemoveComponent<IsAlive>(e);
                        // PostUpdateCommands.DestroyEntity(e);
                    }
                });
        }
    }
}