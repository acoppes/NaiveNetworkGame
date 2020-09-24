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
            
            Entities.ForEach(delegate(Entity e, ref NetworkPlayerState p)
            {
                var networkPlayerState = p;
                
                Entities.ForEach(delegate(ref PlayerController playerController)
                {
                    if (playerController.player != networkPlayerState.player) 
                        return;
                    
                    playerController.gold = networkPlayerState.gold;
                    playerController.player = networkPlayerState.player;
                    playerController.unitType = networkPlayerState.unitType;
                    playerController.currentUnits = networkPlayerState.currentUnits;
                    playerController.maxUnits = networkPlayerState.maxUnits;
                });
                
                PostUpdateCommands.DestroyEntity(e);
            });
        }
    }
}