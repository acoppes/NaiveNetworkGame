using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    // Given a move action in a unit, it processes movement updates

    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial class MovementActionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = SystemAPI.Time.DeltaTime;
            
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            Entities
                .WithNone<SpawningAction, AttackAction>()
                .WithAll<Movement, LocalTransform>()
                // .WithAllReadOnly<MovementAction>()
                .ForEach((Entity e, ref Movement movement, ref LocalTransform t, ref MovementAction m) =>
                {
                    var p0 = t.Position.xy;
                    var p1 = m.target;

                    var speed = movement.speed * dt;

                    if (math.distancesq(p1, p0) < speed * speed)
                    {
                        // already near destination...
                        ecb.RemoveComponent<MovementAction>(e);
                        t.Position = new float3(p1.x, p1.y, t.Position.z);
                        return;
                    }

                    m.direction = math.normalize(p1 - p0);

                    var newpos = p0 + m.direction * movement.speed * dt;

                    if (math.distancesq(p1, p0) < math.distancesq(newpos, p0))
                    {
                        newpos = p1;
                        ecb.RemoveComponent<MovementAction>(e);
                    }

                    t.Position = new float3(newpos.x, newpos.y, t.Position.z);
                }).Run();

            Entities.WithAll<LookingDirection, MovementAction>()
                .ForEach((Entity e, ref LookingDirection d, ref MovementAction m) =>
                {
                    d.direction = m.direction;
                }).Run();

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}