using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Events;

namespace Server
{
    public class ProcessPendingPlayerActionsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var prefabsEntity = 
                Entities.WithAll<PrefabsSharedComponent>().ToEntityQuery().GetSingletonEntity();

            var prefabsSharedComponent = 
                EntityManager.GetSharedComponentData<PrefabsSharedComponent>(prefabsEntity);
            
            var createdUnitsEntity = Entities.WithAll<CreatedUnits>().ToEntityQuery().GetSingletonEntity();
            var createdUnits = EntityManager.GetComponentData<CreatedUnits>(createdUnitsEntity);
            
            // process all player pending actions
            Entities
                .WithNone<ClientOnly>()
                .WithAll<ServerOnly, ClientPlayerAction>()
                .ForEach(delegate (Entity e, ref ClientPlayerAction p)
                {
                    PostUpdateCommands.DestroyEntity(e);

                    var player = p.player;
                    var unitId = p.unit;
                
                    if (p.command == ClientPlayerAction.MoveUnitAction)
                    {
                        var pendingAction = new PendingAction
                        {
                            command = p.command,
                            target = p.target
                        };

                        Entities.WithAll<Unit, Movement>().ForEach(delegate(Entity unitEntity, ref Unit unit)
                        {
                            if (unit.player != player)
                                return;

                            if (unit.id != unitId)
                                return;

                            PostUpdateCommands.RemoveComponent<PendingAction>(unitEntity);
                            PostUpdateCommands.RemoveComponent<MovementAction>(unitEntity);
                            PostUpdateCommands.AddComponent(unitEntity, pendingAction);
                        });
                    } else if (p.command == ClientPlayerAction.CreateUnitAction)
                    {
                        // TODO: create but in spawning state...
                    
                        var unitEntity = PostUpdateCommands.Instantiate(prefabsSharedComponent.unitPrefab);
                        PostUpdateCommands.SetComponent(unitEntity, new Unit
                        {
                            id = (uint) createdUnits.lastCreatedUnitId++,
                            player = p.player
                        });
                        PostUpdateCommands.SetComponent(unitEntity, new Translation
                        {
                            Value = new float3(0, 0, 0)
                            // Value = new float3(p.target.x, p.target.y, 0)
                        });
                        PostUpdateCommands.SetComponent(unitEntity, new UnitState
                        {
                            state = UnitState.spawningState
                        });
                        PostUpdateCommands.AddComponent(unitEntity, new SpawningAction
                        {
                            duration = 1.0f
                        });
                        PostUpdateCommands.AddComponent(unitEntity, new NetworkGameState
                        {
                            // syncVersion = -1
                        });
                    }
                });
            
            PostUpdateCommands.SetComponent(createdUnitsEntity, createdUnits);
        }
    }
}