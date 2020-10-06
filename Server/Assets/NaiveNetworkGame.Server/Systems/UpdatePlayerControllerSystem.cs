using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ServerProcessPendingPlayerActionsSystem))]
    public class UpdatePlayerControllerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(delegate(ref PlayerController p, DynamicBuffer<BuildingSlot> buildingSlots)
            {
                var player = p.player;

                p.currentUnits = 0;
                
                var unitSlots = p.currentUnits;
                
                Entities
                    .WithAll<IsAlive>()
                    .ForEach(delegate(ref Unit unit)
                {
                    if (unit.player == player)
                    {
                        unitSlots += unit.slotCost;
                    }
                });

                p.buildingSlots = 0;
                
                for (var i = 0; i < buildingSlots.Length; i++)
                {
                    if (buildingSlots[i].available)
                        p.buildingSlots++;
                }

                p.currentUnits = unitSlots;
            });
        }
    }
}