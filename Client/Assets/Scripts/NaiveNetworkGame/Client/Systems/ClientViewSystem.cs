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
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ClientViewSystem : ComponentSystem
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

            var unitsQuery = Entities.WithAll<Unit, Translation>().ToEntityQuery();
            var units = unitsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);
            var translations = unitsQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
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
                        // PostUpdateCommands.SetComponent(unitEntities[i], new Translation
                        // {
                        //     Value = new float3(update.translation.x, update.translation.y, 0)
                        // });
                        
                        PostUpdateCommands.SetComponent(unitEntities[i], new UnitState
                        {
                            state = networkGameState.state, 
                            percentage = networkGameState.statePercentage
                        });
                        
                        PostUpdateCommands.SetComponent(unitEntities[i], new LookingDirection
                        {
                            direction = direction
                        });

                        // var currentTranslation = translations[i];
                        //
                        // PostUpdateCommands.SetComponent(unitEntities[i], new TranslationInterpolation
                        // {
                        //     previousTranslation = currentTranslation.Value.xy,
                        //     currentTranslation = networkGameState.translation,
                        //     remoteDelta = networkGameState.delta,
                        //     time = 0
                        // });

                        // var updateBuffer = PostUpdateCommands.SetBuffer<UnitGameState>(unitEntities[i]);
                        // buffer.Add(new UnitGameState
                        // {
                        //     frame = update.frame,
                        //     time = update.time,
                        //     translation = update.translation
                        // });
                        
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
                PostUpdateCommands.AddComponent(entity, new Translation());
                // PostUpdateCommands.AddComponent(entity, new Translation
                // {
                //     Value = new float3(networkGameState.translation.x, networkGameState.translation.y, 0)
                // });
                PostUpdateCommands.AddComponent(entity, new UnitState
                {
                    state = networkGameState.state
                });
                PostUpdateCommands.AddComponent(entity, new LookingDirection
                {
                    direction = direction
                });

                PostUpdateCommands.AddComponent(entity, new TranslationInterpolation
                {
                    previousTranslation = float2.zero,
                    currentTranslation = float2.zero,
                    time = 0,
                    remoteDelta = 0
                });

                if (networkGameState.unitType == 0)
                {
                    PostUpdateCommands.AddComponent(entity, new Selectable());
                }
                
                // var buffer = PostUpdateCommands.AddBuffer<UnitGameState>(entity);
                // buffer.Add(new UnitGameState
                // {
                //     frame = update.frame,
                //     time = update.time,
                //     translation = update.translation
                // });
                
                // PostUpdateCommands.AddComponent(entity, new ClientConnectionId
                // {
                //     id = update.connectionId
                // });
                
                createdUnitsInThisUpdate.Add(networkGameState.unitId);
            }

            translations.Dispose();
            unitEntities.Dispose();
            updateEntities.Dispose();
            units.Dispose();
            updates.Dispose();
        }
    }
}