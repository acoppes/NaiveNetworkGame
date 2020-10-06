using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public class UpdateNetworkPlayerStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // given a network gamestate, update player local data
            
            Entities
                .WithAll<NetworkPlayerState, ClientOnly>()
                .ForEach(delegate(Entity e, ref NetworkPlayerState p)
            {
                var networkPlayerState = p;
                
                Entities
                    .ForEach(delegate(ref LocalPlayerController playerController)
                {
                    if (playerController.player != networkPlayerState.player) 
                        return;
                    
                    playerController.gold = networkPlayerState.gold;
                    playerController.player = networkPlayerState.player;
                    playerController.skinType = networkPlayerState.skinType;
                    playerController.currentUnits = networkPlayerState.currentUnits;
                    playerController.maxUnits = networkPlayerState.maxUnits;
                    playerController.buildingSlots = networkPlayerState.buildingSlots;
                });
                
                PostUpdateCommands.DestroyEntity(e);
            });
        }
    }
}