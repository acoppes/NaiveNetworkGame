using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    // Given a move action in a unit, it processes movement updates

    public class MovementActionSystem : ComponentSystem
    {
        
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities
                .WithNone<SpawningAction, AttackAction>()
                .WithAll<Movement, Translation>()
                .WithAllReadOnly<MovementAction>()
                .ForEach(delegate(Entity e, ref Movement movement, ref Translation t, ref MovementAction m)
                {
                    var p0 = t.Value.xy;
                    var p1 = m.target;

                    var speed = movement.speed * dt;

                    if (math.distancesq(p1, p0) < speed * speed)
                    {
                        // already near destination...
                        PostUpdateCommands.RemoveComponent<MovementAction>(e);
                        t.Value = new float3(p1.x, p1.y, t.Value.z);
                        return;
                    }
                    
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
}