using Client;
using NaiveNetworkGame.Common;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public class UpdateNetworkPlayerStateSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<PlayerController>();
        }
        
        protected override void OnUpdate()
        {
            // given a network gamestate, update player local data
            
            Entities.ForEach(delegate(Entity e, ref NetworkPlayerState p)
            {
                PostUpdateCommands.DestroyEntity(e);

                // we normally don't get other players state...
                var controller = GetSingleton<PlayerController>();
                controller.gold = p.gold;
            });
        }
    }
}