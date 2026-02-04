using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ProcessPendingPlayerActionsSystem))]
    [UpdateBefore(typeof(UpdateNetworkGameStateSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class UpdatePlayerControllerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(delegate(ref PlayerControllerComponentData p)
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

                byte freeBarracksCount = 0;
                
                Entities
                    .WithNone<SpawningAction>()
                    .WithNone<BuildUnitAction>()
                    .ForEach(delegate(ref Barracks b, ref Unit u)
                {
                    if (u.player == player)
                        freeBarracksCount++;
                });

                p.freeBarracksCount = freeBarracksCount;

                byte availableBuildingSlots = 0;
                
                Entities
                    .ForEach(delegate(ref BuildingHolder holder, ref Unit u)
                    {
                        if (u.player != player)
                            return;

                        if (holder.hasBuilding)
                            return;
                        
                        availableBuildingSlots++;
                    });
                p.availableBuildingSlots = availableBuildingSlots;

                p.currentUnits = unitSlots;
            });
        }
    }
}