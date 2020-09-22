using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateBefore(typeof(CreateUnitFromNetworkGameStateSystem))]
    public class DestroyUnitsWithoutNetworkGameStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // iterate over client view updates...
            var gameStateQuery = Entities.WithAll<NetworkGameState>().ToEntityQuery();

            if (gameStateQuery.CalculateEntityCount() == 0)
                return;
            
            var unitsQuery = Entities.WithAll<Unit>().ToEntityQuery();
            
            var units = unitsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);
            var unitEntities = unitsQuery.ToEntityArray(Allocator.TempJob);

            var gameStates = gameStateQuery.ToComponentDataArray<NetworkGameState>(Allocator.TempJob);
            
            for (var i = 0; i < units.Length; i++)
            {
                var hasNetworkSync = false;
                for (var j = 0; j < gameStates.Length; j++)
                {
                    if (units[i].unitId == gameStates[j].unitId)
                    {
                        hasNetworkSync = true;
                        break;
                    }
                }

                if (!hasNetworkSync)
                {
                    PostUpdateCommands.DestroyEntity(unitEntities[i]);
                }
            }

            gameStates.Dispose();
            units.Dispose();
            unitEntities.Dispose();
        }
    }
}