using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
                    
                    // PostUpdateCommands.SetComponent(e, new UnitState
                    // {
                    //     state = 1
                    // });
                });

            Entities.WithAll<LookingDirection, MovementAction>()
                .ForEach(delegate(Entity e, ref LookingDirection d, ref MovementAction m)
                {
                    d.direction = m.direction;
                });

        }
    }

    [UpdateBefore(typeof(UnitStateSystem))]
    public class ServerSpawningActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities.WithAll<SpawningAction>()
                .ForEach(delegate(Entity e, ref SpawningAction s)
                {
                    s.time += dt;

                    if (s.time >= s.duration)
                    {
                        PostUpdateCommands.RemoveComponent<SpawningAction>(e);
                    }
                });

            Entities.WithAll<LookingDirection, MovementAction>()
                .ForEach(delegate(Entity e, ref LookingDirection d, ref MovementAction m)
                {
                    d.direction = m.direction;
                });
        }
    }
    
    // [UpdateBefore(typeof(ServerMovementSystem))]
    public class UnitStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<SpawningAction, UnitState>()
                .ForEach(delegate(Entity e, ref UnitState u, ref SpawningAction s)
                {
                    u.state = UnitState.spawningState;
                    u.percentage = (byte) Mathf.RoundToInt(100.0f * s.time / s.duration);
                });
            
            Entities
                .WithAll<MovementAction, UnitState>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = UnitState.walkState;
                });
            
            Entities
                .WithNone<MovementAction, SpawningAction>()
                .WithAll<UnitState>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = UnitState.idleState;
                });
        }
    }
}