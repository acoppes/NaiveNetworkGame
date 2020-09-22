using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Server;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    public class ProcessPendingPlayerActionsSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<CreatedUnits>();
        }

        protected override void OnUpdate()
        {
            var createdUnitsEntity = GetSingletonEntity<CreatedUnits>();
            var createdUnits = GetSingleton<CreatedUnits>();
            
            // process all player pending actions
            Entities
                .WithAll<ClientPlayerAction, PlayerController, Translation>()
                .ForEach(delegate (Entity e, ref ClientPlayerAction p, ref PlayerController playerController, ref Translation t)
                {
                    // PostUpdateCommands.DestroyEntity(e);
                    PostUpdateCommands.RemoveComponent<ClientPlayerAction>(e);

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
                        // dont create unit if at maximum capacity
                        if (playerController.currentUnits >= playerController.maxUnits) 
                            return;

                        var unitEntity = PostUpdateCommands.Instantiate(playerController.unitPrefab);
                        
                        PostUpdateCommands.SetComponent(unitEntity, new Unit
                        {
                            id = (ushort) createdUnits.lastCreatedUnitId++,
                            player = player,
                            type = playerController.unitType
                        });
                        
                        PostUpdateCommands.SetComponent(unitEntity, new Translation
                        {
                            Value = t.Value
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
                        PostUpdateCommands.AddComponent(unitEntity, new NetworkGameState());
                        PostUpdateCommands.AddComponent(unitEntity, new NetworkTranslationSync());
                    }
                });
            
            PostUpdateCommands.SetComponent(createdUnitsEntity, createdUnits);
        }
    }
}