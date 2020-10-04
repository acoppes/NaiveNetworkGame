using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public class ResourceCollectionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<IsAlive, ResourceCollector, ServerOnly>()
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
    public class CopyResourceCollectionToPlayerControllerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<PlayerController, ServerOnly>()
                .ForEach(delegate(ref PlayerController p)
                {
                    var playerId = p.player;
                    var gold = p.gold;
                    Entities
                        .WithAll<IsAlive, Unit, ResourceCollector, ServerOnly>()
                        .ForEach(delegate(ref Unit unit, ref ResourceCollector r)
                        {
                            if (unit.player == playerId)
                            {
                                gold += r.collectedGold;
                                r.collectedGold = 0;
                            }
                        });
                    p.gold = gold;
                });
        }
    }
}