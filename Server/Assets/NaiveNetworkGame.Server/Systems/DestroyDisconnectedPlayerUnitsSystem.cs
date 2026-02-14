using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public partial struct DestroyDisconnectedPlayerUnitsSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (playerController, playerEntity) in 
                SystemAPI.Query<RefRO<PlayerController>>()
                    .WithNone<PlayerConnectionId>()
                    .WithEntityAccess())
            {
                var player = playerController.ValueRO.player;
                
                // destroy all player units if no connection
                foreach (var (unit, unitEntity) in 
                    SystemAPI.Query<RefRO<Unit>>()
                        .WithAll<NetworkUnit>()
                        .WithEntityAccess())
                {
                    if (unit.ValueRO.player == player)
                    {
                        ecb.DestroyEntity(unitEntity);
                    }
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
            // Stop sending dead units gamestate sync...
            // var deadUnitsQuery = SystemAPI.QueryBuilder()
            //     .WithAll<ServerOnly, NetworkTranslationSync, NetworkGameState>()
            //     .WithNone<IsAlive>()
            //     .Build();
            // state.EntityManager.RemoveComponent<NetworkTranslationSync>(deadUnitsQuery);
            // state.EntityManager.RemoveComponent<NetworkGameState>(deadUnitsQuery);
        }
    }
}
