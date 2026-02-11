using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public partial struct DestroyDisconnectedPlayerUnitsSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
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
                        state.EntityManager.DestroyEntity(unitEntity);
                    }
                }
            }
            
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
