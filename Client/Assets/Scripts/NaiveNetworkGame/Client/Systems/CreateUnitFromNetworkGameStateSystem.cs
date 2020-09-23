using System.Collections.Generic;
using Client;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Client.Systems
{
    public class CreateUnitFromNetworkGameStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // iterate over client view updates...
            
            var unitsQuery = Entities.WithAll<Unit>().ToEntityQuery();
            var units = unitsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);

            var modelProvider = ModelProviderSingleton.Instance;
            
            var createdUnitsInThisUpdate = new List<int>();
            
            // first create entities to be updated....
            
            Entities.WithAll<NetworkGameState>()
                .ForEach(delegate(ref NetworkGameState n)
            {
                if (createdUnitsInThisUpdate.Contains(n.unitId))
                    return;
                
                for (var i = 0; i < units.Length; i++)
                {
                    var unit = units[i];
                    if (unit.unitId == n.unitId)
                    {
                        // unit already created before...
                        return;
                    }
                }

                // create visual model for this unit
                var entity = PostUpdateCommands.CreateEntity();
                PostUpdateCommands.AddComponent(entity, new Unit
                {
                    unitId = n.unitId,
                    player = n.playerId
                });
                
                PostUpdateCommands.AddComponent(entity, new HealthPercentage
                {
                    value = n.health
                });

                PostUpdateCommands.AddSharedComponent(entity, new ModelPrefabComponent
                {
                    prefab = modelProvider.prefabs[n.unitType]
                });
                
                // create it far away first time...
                PostUpdateCommands.AddComponent(entity, new Translation
                {
                    Value = new float3(100, 100, 0)
                });
                
                PostUpdateCommands.AddComponent(entity, new UnitState());
                PostUpdateCommands.AddComponent(entity, new LookingDirection());
                // PostUpdateCommands.AddComponent(entity, new Selectable());
                PostUpdateCommands.AddComponent(entity, new NetworkObject());

                createdUnitsInThisUpdate.Add(n.unitId);
            });

            units.Dispose();
        }
    }
}