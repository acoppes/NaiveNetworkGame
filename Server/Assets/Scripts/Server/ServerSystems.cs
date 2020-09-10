using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Server
{
    public class UnitBehaviourSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Unit, Movement>()
                .WithNone<MovementAction, SpawningAction, IdleAction>()
                .ForEach(delegate (Entity e)
            {
                PostUpdateCommands.AddComponent(e, new MovementAction
                {
                    target = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, 1.25f)
                });
                PostUpdateCommands.AddComponent(e, new IdleAction
                {
                    time = UnityEngine.Random.Range(1.0f, 3.0f)
                });
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

    public class MovementActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities
                .WithNone<SpawningAction>()
                .WithAll<Movement, Translation>()
                .WithAllReadOnly<MovementAction>()
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
                });

            Entities.WithAll<LookingDirection, MovementAction>()
                .ForEach(delegate(Entity e, ref LookingDirection d, ref MovementAction m)
                {
                    d.direction = m.direction;
                });

        }
    }

    [UpdateBefore(typeof(UnitStateSystem))]
    public class SpawningActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities
                .WithAll<SpawningAction>()
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
    
    public class IdleActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities
                .WithNone<MovementAction, SpawningAction>()
                .ForEach(delegate(Entity e, ref IdleAction idle)
                {
                    idle.time -= dt;

                    if (idle.time < 0)
                    {
                        PostUpdateCommands.RemoveComponent<IdleAction>(e);
                    }
                });
        }
    }
    
    // [UpdateBefore(typeof(ServerMovementSystem))]
    public class UnitStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<MovementAction>()
                .WithAll<SpawningAction, UnitState>()
                .ForEach(delegate(Entity e, ref UnitState u, ref SpawningAction s)
                {
                    u.state = UnitState.spawningState;
                    u.percentage = (byte) Mathf.RoundToInt(100.0f * s.time / s.duration);
                });
            
            Entities
                .WithNone<SpawningAction>()
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