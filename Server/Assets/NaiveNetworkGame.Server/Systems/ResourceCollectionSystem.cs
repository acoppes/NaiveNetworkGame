using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct ResourceCollectionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var resourceCollector in 
                SystemAPI.Query<RefRW<ResourceCollector>>()
                    .WithNone<SpawningAction>()
                    .WithAll<IsAlive, ServerOnly>())
            {
                resourceCollector.ValueRW.currentCollectionTime += SystemAPI.Time.DeltaTime;
                if (resourceCollector.ValueRO.currentCollectionTime > 1)
                {
                    resourceCollector.ValueRW.currentCollectionTime -= 1;
                    resourceCollector.ValueRW.collectedGold += resourceCollector.ValueRO.goldPerSecond;
                }
            }
        }
    }

    [UpdateAfter(typeof(ResourceCollectionSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct CopyResourceCollectionToPlayerControllerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerController in 
                SystemAPI.Query<RefRW<PlayerController>>()
                    .WithAll<ServerOnly>())
            {
                var playerId = playerController.ValueRO.player;
                var gold = playerController.ValueRO.gold;
                byte maxUnits = 0;
                
                foreach (var (unit, resourceCollector) in 
                    SystemAPI.Query<RefRO<Unit>, RefRW<ResourceCollector>>()
                        .WithNone<SpawningAction>()
                        .WithAll<IsAlive, ServerOnly>())
                {
                    if (unit.ValueRO.player == playerId)
                    {
                        gold += resourceCollector.ValueRO.collectedGold;
                        resourceCollector.ValueRW.collectedGold = 0;
                    }
                }
                
                foreach (var (unit, house) in 
                    SystemAPI.Query<RefRO<Unit>, RefRO<House>>()
                        .WithNone<SpawningAction>()
                        .WithAll<IsAlive, ServerOnly>())
                {
                    if (unit.ValueRO.player == playerId)
                    {
                        maxUnits += house.ValueRO.maxUnits;
                    }
                }

                playerController.ValueRW.maxUnits = maxUnits;

                if (gold > playerController.ValueRO.maxGold)
                    gold = playerController.ValueRO.maxGold;
                
                playerController.ValueRW.gold = gold;
            }
        }
    }
}
