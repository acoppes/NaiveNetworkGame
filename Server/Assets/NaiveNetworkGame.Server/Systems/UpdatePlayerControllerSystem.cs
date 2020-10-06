using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ServerProcessPendingPlayerActionsSystem))]
    public class UpdatePlayerControllerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(delegate(ref PlayerController p)
            {
                var player = p.player;

                p.currentUnits = 0;
                
                var slots = p.currentUnits;
                
                Entities
                    .WithAll<IsAlive>()
                    .ForEach(delegate(ref Unit unit)
                {
                    if (unit.player == player)
                    {
                        slots += unit.slotCost;
                    }
                });

                p.currentUnits = slots;
            });
        }
    }
}