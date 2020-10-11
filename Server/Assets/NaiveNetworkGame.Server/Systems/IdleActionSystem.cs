using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public class IdleActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            // if for some reason we have a movement action pending... remove idle
            // Entities
            //     .WithAll<IdleAction, MovementAction>()
            //     .ForEach(delegate(Entity e)
            //     {
            //         PostUpdateCommands.RemoveComponent<IdleAction>(e);
            //     });
            
            // if for some reason we have an attack action pending... remove idle
            Entities
                .WithAll<IdleAction, AttackAction>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.RemoveComponent<IdleAction>(e);
                });
            
            Entities
                .WithNone<MovementAction, SpawningAction, AttackAction, DeathAction>()
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