using NaiveNetworkGame.Server.Components;
using Server;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public class IdleActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities
                .WithNone<MovementAction, SpawningAction>()
                .WithAll<IdleAction>()
                .ForEach(delegate(Entity e, ref IdleAction idle)
                {
                    idle.time -= dt;

                    if (idle.time < 0)
                    {
                        PostUpdateCommands.RemoveComponent<IdleAction>(e);
                    }
                });
        }
    }
}