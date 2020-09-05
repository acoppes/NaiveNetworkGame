using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Server
{
    public class ServerIncomingCommandsFromNetworkSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<ClientOnly>()
                .WithAll<ServerOnly, NetworkManagerSharedComponent, ServerRunningComponent>()
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent s)
            {
                var manager = s.networkManager;

                var connectionCount = manager.m_Connections.Length;
                
                // Debug.Log($"Connections: {connectionCount}");
                
                // manager.connections....

                // create locally interesting commands for the game
            });
        }
    }
    
    // public class ServerProcessIncomingCommandsSystem : ComponentSystem
    // {
    //     protected override void OnUpdate()
    //     {
    //         Entities.WithAll<ServerComponent>().ForEach(delegate(Entity e, ServerComponent s)
    //         {
    //             var manager = s.manager;
    //             // manager.connections....
    //             
    //             // create pending player action commands...
    //         });
    //     }
    // }
    
    public class ServerOutgoingGameStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<NetworkManagerSharedComponent>().ForEach(delegate(Entity e, NetworkManagerSharedComponent s)
            {
                var manager = s.networkManager;
                // manager.connections....
                
                // given the game state (entities with some interesting commands to share)
                // send packets to each client...
            });
        }
    }

    public class ServerPendingPlayerActionsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var playerControllerPrefabEntity = 
                Entities.WithAll<PlayerControllerSharedComponent>().ToEntityQuery().GetSingletonEntity();

            var playerControllerPrefab = 
                EntityManager.GetSharedComponentData<PlayerControllerSharedComponent>(playerControllerPrefabEntity);
            
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
                    
                    var playerControllerEntity = PostUpdateCommands.Instantiate(playerControllerPrefab.unitPrefab);
                    PostUpdateCommands.SetComponent(playerControllerEntity, new Unit
                    {
                        id = (uint) createdUnits.lastCreatedUnitId++,
                        player = p.player
                    });
                    PostUpdateCommands.SetComponent(playerControllerEntity, new Translation
                    {
                        Value = new float3(p.target.x, p.target.y, 0)
                    });
                    PostUpdateCommands.AddComponent(playerControllerEntity, new NetworkGameState
                    {
                        // syncVersion = -1
                    });
                }
            });
            
            PostUpdateCommands.SetComponent(createdUnitsEntity, createdUnits);
        }
    }
    
    public class ServerUnitPendingActionsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<PendingAction, Movement>().WithNone<MovementAction>().ForEach(delegate (Entity e, ref PendingAction p)
            {
                PostUpdateCommands.RemoveComponent<PendingAction>(e);
                
                // movement command
                if (p.command == ClientPlayerAction.MoveUnitAction)
                {
                    PostUpdateCommands.AddComponent(e, new MovementAction
                    {
                        target = p.target
                    });
                }
            });
        }
    }

    public class ServerMovementSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities.WithAll<Movement, Translation>().WithAllReadOnly<MovementAction>()
                .ForEach(delegate(Entity e, ref Movement movement, ref Translation t, ref MovementAction m)
                {
                    var p0 = t.Value.xy;
                    var p1 = m.target;

                    m.direction = math.normalize(p1 - p0);
                    
                    var newpos = p0 + m.direction * movement.speed * dt;

                    if (math.distancesq(p1, p0) < math.distancesq(newpos, p0))
                    {
                        newpos = p1;
                        PostUpdateCommands.RemoveComponent<MovementAction>(e);
                    }

                    t.Value = new float3(newpos.x, newpos.y, t.Value.z);
                    
                    PostUpdateCommands.SetComponent(e, new UnitState
                    {
                        state = 1
                    });
                });

            Entities.WithAll<LookingDirection, MovementAction>()
                .ForEach(delegate(Entity e, ref LookingDirection d, ref MovementAction m)
                {
                    d.direction = m.direction;
                });

        }
    }
    
    // [UpdateBefore(typeof(ServerMovementSystem))]
    public class ServerUnitStateComponent : ComponentSystem
    {
        // Recover unit idle state system...
        
        protected override void OnUpdate()
        {
            // var dt = Time.DeltaTime;
            
            Entities
                .WithAll<MovementAction, UnitState>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = 1;
                });
            
            Entities
                .WithNone<MovementAction>()
                .WithAll<UnitState>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = 0;
                });
            
            // Entities
            //     .ForEach(delegate(Entity e, ref UnitState unitState)
            //     {
            //         unitState.state = 0;
            //     });
        }
    }
}