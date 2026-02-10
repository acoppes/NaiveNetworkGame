using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ProcessPendingPlayerActionsSystem))]
    [UpdateBefore(typeof(UpdateNetworkGameStateSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial class UpdatePlayerControllerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            foreach (var playerRW in SystemAPI.Query<RefRW<PlayerController>>())
            {
                ref var p = ref playerRW.ValueRW;
                
                var player = p.player;
                p.currentUnits = 0;
                var unitSlots = p.currentUnits;

                foreach (var unitRW in SystemAPI.Query<RefRW<Unit>>().WithAll<IsAlive>())
                {
                    ref var unit = ref unitRW.ValueRW;
                    if (unit.player == player)
                    {
                        unitSlots += unit.slotCost;
                    }
                }
                p.currentUnits = unitSlots;
                
                byte freeBarracksCount = 0;
                foreach (var (b, u) in SystemAPI.Query<Barracks, Unit>()
                             .WithNone<SpawningAction>()
                             .WithNone<BuildUnitAction>())
                {
                    if (u.player == player)
                        freeBarracksCount++;
                }
                p.freeBarracksCount = freeBarracksCount;
                
                byte availableBuildingSlots = 0;
                
                foreach (var (holder, u) in SystemAPI.Query<BuildingHolder, Unit>()
                             .WithNone<SpawningAction>()
                             .WithNone<BuildUnitAction>())
                {
                    if (u.player != player)
                        continue;

                    if (holder.hasBuilding)
                        continue;

                    availableBuildingSlots++;
                }
                p.availableBuildingSlots = availableBuildingSlots;
            }
            
            // Entities.ForEach((ref PlayerController p) =>
            // {
            //     var player = p.player;
            //
            //     p.currentUnits = 0;
            //
            //     var unitSlots = p.currentUnits;
            //
            //     Entities
            //         .WithAll<IsAlive>()
            //         .ForEach((ref Unit unit) =>
            //         {
            //             if (unit.player == player)
            //             {
            //                 unitSlots += unit.slotCost;
            //             }
            //         }).Run();
            //
            //     byte freeBarracksCount = 0;
            //
            //     Entities
            //         .WithNone<SpawningAction>()
            //         .WithNone<BuildUnitAction>()
            //         .ForEach((ref Barracks b, ref Unit u) =>
            //         {
            //             if (u.player == player)
            //                 freeBarracksCount++;
            //         }).Run();
            //
            //     p.freeBarracksCount = freeBarracksCount;
            //
            //     byte availableBuildingSlots = 0;
            //
            //     Entities
            //         .ForEach((ref BuildingHolder holder, ref Unit u) =>
            //         {
            //             if (u.player != player)
            //                 return;
            //
            //             if (holder.hasBuilding)
            //                 return;
            //
            //             availableBuildingSlots++;
            //         }).Run();
            //     p.availableBuildingSlots = availableBuildingSlots;
            //
            //     p.currentUnits = unitSlots;
            // }).Run();
        }
    }
}