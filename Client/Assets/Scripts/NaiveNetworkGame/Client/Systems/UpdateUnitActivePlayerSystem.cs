using NaiveNetworkGame.Client.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    // for each unit, it updates if the unit is owned by local player
    public partial struct UpdateUnitActivePlayerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerController in 
                SystemAPI.Query<RefRO<LocalPlayerController>>())
            {
                var player = playerController.ValueRO.player;
                
                foreach (var unit in 
                    SystemAPI.Query<RefRW<Unit>>())
                {
                    unit.ValueRW.isLocalPlayer = unit.ValueRO.player == player;
                }
            }
        }
    }
}
