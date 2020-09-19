using System.Collections.Generic;
using Client;
using Mockups;
using NaiveNetworkGame.Common;
using Scenes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    // TODO: separate in system to create models for units that don't have one
    // TODO: and system to update unit game state from network (like the other system for translation sync)
    
    public class CreateUnitVisualModelFromNetworkGameStateSystem : ComponentSystem
    {
        private Vector2 Vector2FromAngle(float a)
        {
            a *= Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }
        
        protected override void OnUpdate()
        {
            // iterate over client view updates...

            var query = Entities.WithAll<NetworkGameState>().ToEntityQuery();
            
            var updates = query.ToComponentDataArray<NetworkGameState>(Allocator.TempJob);
            var updateEntities = query.ToEntityArray(Allocator.TempJob);

            var unitsQuery = Entities.WithAll<Unit>().ToEntityQuery();
            var units = unitsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);
            
            var unitEntities = unitsQuery.ToEntityArray(Allocator.TempJob);
            
            var createdUnitsInThisUpdate = new List<int>();
            
            // first create entities to be updated....

            for (var j = 0; j < updates.Length; j++)
            {
                PostUpdateCommands.DestroyEntity(updateEntities[j]);
                
                var networkGameState = updates[j];
                var updated = false;

                if (createdUnitsInThisUpdate.Contains(networkGameState.unitId))
                    continue;

                var direction = Vector2FromAngle(networkGameState.lookingDirectionAngleInDegrees);

                for (var i = 0; i < units.Length; i++)
                {
                    var unit = units[i];
                   
                    if (unit.unitId == networkGameState.unitId)
                    {
                        PostUpdateCommands.SetComponent(unitEntities[i], new UnitState
                        {
                            state = networkGameState.state, 
                            percentage = networkGameState.statePercentage
                        });
                        
                        PostUpdateCommands.SetComponent(unitEntities[i], new LookingDirection
                        {
                            direction = direction
                        });

                        updated = true;
                        break;
                    }
                }

                if (updated)
                    continue;
                
                var modelProvider = ModelProviderSingleton.Instance;
                    
                // create visual model for this unit
                var entity = PostUpdateCommands.CreateEntity();
                PostUpdateCommands.AddComponent(entity, new Unit
                {
                    unitId = networkGameState.unitId,
                    player = networkGameState.playerId
                });
                PostUpdateCommands.AddSharedComponent(entity, new ModelPrefabComponent
                {
                    prefab = modelProvider.prefabs[networkGameState.unitType]
                });
                
                // create it far away first time...
                PostUpdateCommands.AddComponent(entity, new Translation()
                {
                    Value = new float3(100, 100, 0)
                });
                
                PostUpdateCommands.AddComponent(entity, new UnitState
                {
                    state = networkGameState.state
                });
                
                PostUpdateCommands.AddComponent(entity, new LookingDirection
                {
                    direction = direction
                });
                
                if (networkGameState.unitType == 0)
                {
                    PostUpdateCommands.AddComponent(entity, new Selectable());
                }
                
                createdUnitsInThisUpdate.Add(networkGameState.unitId);
            }

            unitEntities.Dispose();
            updateEntities.Dispose();
            units.Dispose();
            updates.Dispose();
        }
    }
}