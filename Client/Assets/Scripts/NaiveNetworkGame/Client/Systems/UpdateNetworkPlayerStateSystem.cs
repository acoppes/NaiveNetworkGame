using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public partial struct UpdateNetworkPlayerStateSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // given a network gamestate, update player local data
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (networkPlayerState, entity) in 
                SystemAPI.Query<RefRO<NetworkPlayerState>>()
                    .WithAll<ClientOnly>()
                    .WithEntityAccess())
            {
                var networkState = networkPlayerState.ValueRO;
                
                foreach (var playerController in 
                    SystemAPI.Query<RefRW<LocalPlayerController>>())
                {
                    if (playerController.ValueRO.player != networkState.player) 
                        continue;
                    
                    playerController.ValueRW.gold = networkState.gold;
                    playerController.ValueRW.player = networkState.player;
                    playerController.ValueRW.skinType = networkState.skinType;
                    playerController.ValueRW.currentUnits = networkState.currentUnits;
                    playerController.ValueRW.maxUnits = networkState.maxUnits;
                    playerController.ValueRW.buildingSlots = networkState.buildingSlots;
                    playerController.ValueRW.freeBarracksCount = networkState.freeBarracks;
                    playerController.ValueRW.behaviourMode = networkState.behaviourMode;
                }
                
                ecb.DestroyEntity(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
