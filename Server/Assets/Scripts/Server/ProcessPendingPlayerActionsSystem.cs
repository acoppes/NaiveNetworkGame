using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Transforms;

namespace Server
{
    public class ProcessPendingPlayerActionsSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<CreatedUnits>();
            RequireSingletonForUpdate<PrefabsSharedComponent>();
        }

        protected override void OnUpdate()
        {
            var prefabsEntity = GetSingletonEntity<PrefabsSharedComponent>();
            var prefabsSharedComponent = 
                EntityManager.GetSharedComponentData<PrefabsSharedComponent>(prefabsEntity);

            var createdUnitsEntity = GetSingletonEntity<CreatedUnits>();
            var createdUnits = GetSingleton<CreatedUnits>();
            
            // process all player pending actions
            Entities
                .WithAll<ClientPlayerAction>()
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

                        var spawnPositionEntity = Entities
                            .WithAll<PlayerController, Translation>()
                            .ToEntityQuery()
                            .TryGetFirstReadOnly<PlayerController>(
                            p => p.player == player);

                        var spawnPosition = EntityManager.GetComponentData<Translation>(spawnPositionEntity).Value;
                    
                        var unitEntity = PostUpdateCommands.Instantiate(prefabsSharedComponent.unitPrefab);
                        PostUpdateCommands.SetComponent(unitEntity, new Unit
                        {
                            id = (ushort) createdUnits.lastCreatedUnitId++,
                            player = player
                        });
                        PostUpdateCommands.SetComponent(unitEntity, new Translation
                        {
                            Value = spawnPosition
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