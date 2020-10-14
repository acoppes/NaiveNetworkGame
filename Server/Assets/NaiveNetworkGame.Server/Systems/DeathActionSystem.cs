using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class DeathActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<DeathAction, MovementAction>()
                .ForEach(delegate(Entity e, ref DeathAction a)
                {
                    PostUpdateCommands.RemoveComponent<MovementAction>(e);
                });
            
            
            Entities
                .WithAll<DeathAction, ChaseTarget>()
                .ForEach(delegate(Entity e, ref DeathAction a)
                {
                    PostUpdateCommands.RemoveComponent<ChaseTarget>(e);
                });

            Entities
                .WithNone<IsAlive>()
                .WithAll<DeathAction>()
                .ForEach(delegate(Entity e, ref DeathAction a)
                {
                    a.time += Time.DeltaTime;
                    if (a.time > a.duration)
                    {
                        // set completely death?
                        PostUpdateCommands.RemoveComponent<DeathAction>(e);
                        
                        // TODO: for now we are sending gamestate of death units too to keep corpses in client...
                        //PostUpdateCommands.RemoveComponent<NetworkTranslationSync>(e);
                        PostUpdateCommands.RemoveComponent<NetworkGameState>(e);
                    }
                });
        }
    }
}