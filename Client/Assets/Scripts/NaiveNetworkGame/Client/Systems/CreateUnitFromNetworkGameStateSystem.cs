using System.Collections.Generic;
using Client;
using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Client.Systems
{
    public partial struct CreateUnitFromNetworkGameStateSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // iterate over client view updates...
            
            var unitsQuery = SystemAPI.QueryBuilder().WithAll<Unit>().Build();
            var units = unitsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);

            var modelProvider = ModelProviderSingleton.Instance;
            
            var createdUnitsInThisUpdate = new List<int>();
            
            // first create entities to be updated....
            
            foreach (var networkGameState in 
                SystemAPI.Query<RefRO<NetworkGameState>>()
                    .WithAll<ClientOnly>())
            {
                // Ignore unit id 0 which is reserved for no entity
                if (networkGameState.ValueRO.unitId == 0)
                    continue;
                
                if (createdUnitsInThisUpdate.Contains(networkGameState.ValueRO.unitId))
                    continue;

                for (var i = 0; i < units.Length; i++)
                {
                    var unit = units[i];
                    if (unit.unitId == networkGameState.ValueRO.unitId)
                    {
                        // unit already created before...
                        goto NextNetworkState;
                    }
                }

                // create visual model for this unit
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new Unit
                {
                    unitId = networkGameState.ValueRO.unitId,
                    player = networkGameState.ValueRO.playerId
                });
                
                state.EntityManager.AddComponentData(entity, new HealthPercentage
                {
                    value = networkGameState.ValueRO.health
                });

                // var playerId = networkGameState.ValueRO.playerId;
                var skinType = networkGameState.ValueRO.skinType;
                
                var skinModels = modelProvider.skinModels[skinType];
                var modelPrefab = skinModels.list[networkGameState.ValueRO.unitType];
                
                state.EntityManager.AddSharedComponentManaged(entity, new ModelPrefabComponent
                {
                    prefab = modelPrefab
                });
                
                // create it far away first time...
                state.EntityManager.AddComponentData(entity, new LocalTransform
                {
                    Position = new float3(100, 100, 0),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                
                state.EntityManager.AddComponentData(entity, new UnitStateComponent());
                state.EntityManager.AddComponentData(entity, new LookingDirection());
                // state.EntityManager.AddComponent(entity, new Selectable());
                state.EntityManager.AddComponentData(entity, new NetworkObject());
                state.EntityManager.AddComponent<ClientOnly>(entity);

                createdUnitsInThisUpdate.Add(networkGameState.ValueRO.unitId);
                
                NextNetworkState:;
            }

            units.Dispose();
        }
    }
}
