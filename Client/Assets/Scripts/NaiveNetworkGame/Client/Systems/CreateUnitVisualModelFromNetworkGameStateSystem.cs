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
    public class CreateUnitVisualModelFromNetworkGameStateSystem : ComponentSystem
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
                PostUpdateCommands.AddComponent(entity, new Selectable());

                createdUnitsInThisUpdate.Add(n.unitId);
            });
            
            units.Dispose();
        }
    }
    
    [UpdateAfter(typeof(CreateUnitVisualModelFromNetworkGameStateSystem))]
    public class UpdateUnitFromNetworkGameStateSystem : ComponentSystem
    {
        private Vector2 Vector2FromAngle(float a)
        {
            a *= Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }
        
        protected override void OnUpdate()
        {
            // TODO: separate in different network state syncs too

            // updates all created units with network state...
            
            Entities
                .WithAll<NetworkGameState>()
                .ForEach(delegate(Entity e, ref NetworkGameState n)
                {
                    // var uid = n.unitId;
                    var ngs = n;
                    
                    Entities
                        .WithAll<Unit, LookingDirection>()
                        .ForEach(delegate(ref Unit u, ref UnitState us, ref LookingDirection l)
                        {
                            if (u.unitId != ngs.unitId)
                                return;

                            us.state = ngs.state;
                            us.percentage = ngs.statePercentage;
                            
                            l.direction = Vector2FromAngle(ngs.lookingDirectionAngleInDegrees);
                        });
                
                PostUpdateCommands.DestroyEntity(e);
            });
        }
    }
}