using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ServerProcessPendingPlayerActionsSystem))]
    [UpdateBefore(typeof(UpdateNetworkGameStateSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
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

                p.availableBuildingSlots = 0;

                byte barracksCount = 0;
                
                Entities.ForEach(delegate(ref Barracks b, ref Unit u)
                {
                    if (u.player == player)
                        barracksCount++;
                });

                p.barracksCount = barracksCount;
                
                for (var i = 0; i < buildingSlots.Length; i++)
                {
                    if (!buildingSlots[i].hasBuilding)
                    {
                        p.availableBuildingSlots++;
                    }
                }

                p.currentUnits = unitSlots;
            });
        }
    }
}