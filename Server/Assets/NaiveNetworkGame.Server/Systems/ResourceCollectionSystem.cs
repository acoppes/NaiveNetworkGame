using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class ResourceCollectionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<SpawningAction>()
                .WithAll<IsAlive, ResourceCollector, ServerOnlyComponentData>()
                .ForEach(delegate(ref ResourceCollector r)
                {
                    r.currentCollectionTime += World.Time.DeltaTime;
                    if (r.currentCollectionTime > 1)
                    {
                        r.currentCollectionTime -= 1;
                        r.collectedGold += r.goldPerSecond;
                    }
                });
        }
    }

    [UpdateAfter(typeof(ResourceCollectionSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class CopyResourceCollectionToPlayerControllerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<PlayerControllerComponentData, ServerOnlyComponentData>()
                .ForEach(delegate(ref PlayerControllerComponentData p)
                {
                    var playerId = p.player;
                    var gold = p.gold;
                    byte maxUnits = 0;
                    
                    Entities
                        .WithNone<SpawningAction>()
                        .WithAll<IsAlive, Unit, ResourceCollector, ServerOnlyComponentData>()
                        .ForEach(delegate(ref Unit unit, ref ResourceCollector r)
                        {
                            if (unit.player == playerId)
                            {
                                gold += r.collectedGold;
                                r.collectedGold = 0;
                            }
                        });
                    
                    Entities
                        .WithNone<SpawningAction>()
                        .WithAll<IsAlive, Unit, House, ServerOnlyComponentData>()
                        .ForEach(delegate(ref Unit unit, ref House h)
                        {
                            if (unit.player == playerId)
                            {
                                maxUnits += h.maxUnits;
                            }
                        });

                    p.maxUnits = maxUnits;

                    if (gold > p.maxGold)
                        gold = p.maxGold;
                    
                    p.gold = gold;
                });
        }
    }
}