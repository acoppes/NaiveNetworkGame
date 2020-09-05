using System.Collections.Generic;
using Client;
using Mockups;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Scenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ClientViewSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // iterate over client view updates...

            var query = Entities.WithAll<NetworkGameState>().ToEntityQuery();
            
            var updates = query.ToComponentDataArray<NetworkGameState>(Allocator.TempJob);
            var updateEntities = query.ToEntityArray(Allocator.TempJob);

            var unitsQuery = Entities.WithAll<UnitComponent, Translation>().ToEntityQuery();
            var units = unitsQuery.ToComponentDataArray<UnitComponent>(Allocator.TempJob);
            var translations = unitsQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var unitEntities = unitsQuery.ToEntityArray(Allocator.TempJob);
            
            var createdUnitsInThisUpdate = new List<int>();
            
            // first create entities to be updated....

            for (var j = 0; j < updates.Length; j++)
            {
                PostUpdateCommands.DestroyEntity(updateEntities[j]);
                
                var update = updates[j];
                var updated = false;

                if (createdUnitsInThisUpdate.Contains(update.unitId))
                    continue;

                for (var i = 0; i < units.Length; i++)
                {
                    var unit = units[i];
                   
                    if (unit.unitId == update.unitId)
                    {
                        // PostUpdateCommands.SetComponent(unitEntities[i], new Translation
                        // {
                        //     Value = new float3(update.translation.x, update.translation.y, 0)
                        // });
                        
                        PostUpdateCommands.SetComponent(unitEntities[i], new UnitState
                        {
                            state = update.state
                        });
                        PostUpdateCommands.SetComponent(unitEntities[i], new LookingDirection
                        {
                            direction = update.lookingDirection
                        });

                        var currentTranslation = translations[i];
                        
                        PostUpdateCommands.SetComponent(unitEntities[i], new UnitGameStateInterpolation
                        {
                            previousTranslation = currentTranslation.Value.xy,
                            currentTranslation = update.translation,
                            remoteDelta = update.delta,
                            time = 0
                        });

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
                PostUpdateCommands.AddComponent(entity, new UnitComponent
                {
                    unitId = (uint) update.unitId,
                    player = (uint) update.playerId
                });
                PostUpdateCommands.AddSharedComponent(entity, new ModelPrefabComponent
                {
                    prefab = modelProvider.prefabs[update.unitType]
                });
                PostUpdateCommands.AddComponent(entity, new Translation
                {
                    Value = new float3(update.translation.x, update.translation.y, 0)
                });
                PostUpdateCommands.AddComponent(entity, new UnitState
                {
                    state = update.state
                });
                PostUpdateCommands.AddComponent(entity, new LookingDirection
                {
                    direction = update.lookingDirection
                });

                PostUpdateCommands.AddComponent(entity, new UnitGameStateInterpolation
                {
                    previousTranslation = update.translation,
                    currentTranslation = update.translation,
                    time = 0,
                    remoteDelta = update.delta
                });

                if (update.unitType == 0)
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
                
                createdUnitsInThisUpdate.Add(update.unitId);
            }

            translations.Dispose();
            unitEntities.Dispose();
            updateEntities.Dispose();
            units.Dispose();
            updates.Dispose();
        }
    }
}