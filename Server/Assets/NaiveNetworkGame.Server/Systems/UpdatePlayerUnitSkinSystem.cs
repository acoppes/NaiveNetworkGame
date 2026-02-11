using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ProcessBuildUnitActionSystem))]
    public partial struct UpdatePlayerUnitSkinSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerController in 
                SystemAPI.Query<RefRO<PlayerController>>())
            {
                var player = playerController.ValueRO.player;
                var skinType = playerController.ValueRO.skinType;
                
                // This system only allows one skin per player (can't combine)
                
                foreach (var (unit, skin) in 
                    SystemAPI.Query<RefRO<Unit>, RefRW<Skin>>())
                {
                    if (unit.ValueRO.player == player)
                    {
                        skin.ValueRW.type = skinType;
                    }
                }
            }
        }
    }
}