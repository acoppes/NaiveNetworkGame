using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    // Given a move action in a unit, it processes movement updates

    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct MovementActionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            foreach (var (t, movement, m, e) in 
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<Movement>, RefRW<MovementAction>>()
                         .WithNone<SpawningAction, AttackAction>()
                         .WithEntityAccess())
            {
                var p0 = t.ValueRW.Position.xy;
                var p1 = m.ValueRO.target;

                var speed = movement.ValueRO.speed * dt;

                if (math.distancesq(p1, p0) < speed * speed)
                {
                    // already near destination...
                    ecb.RemoveComponent<MovementAction>(e);
                    t.ValueRW.Position = new float3(p1.x, p1.y, t.ValueRO.Position.z);
                    continue;
                }

                m.ValueRW.direction = math.normalize(p1 - p0);

                var newpos = p0 + m.ValueRW.direction * movement.ValueRO.speed * dt;

                if (math.distancesq(p1, p0) < math.distancesq(newpos, p0))
                {
                    newpos = p1;
                    ecb.RemoveComponent<MovementAction>(e);
                }

                t.ValueRW.Position = new float3(newpos.x, newpos.y, t.ValueRW.Position.z);
            }
            
            // Entities
            //     .WithNone<SpawningAction, AttackAction>()
            //     .WithAll<Movement, LocalTransform>()
            //     // .WithAllReadOnly<MovementAction>()
            //     .ForEach((Entity e, ref Movement movement, ref LocalTransform t, ref MovementAction m) =>
            //     {
            //
            //     }).Run();

            foreach (var (d, m) in SystemAPI.Query<RefRW<LookingDirection>, RefRO<MovementAction>>())
            {
                d.ValueRW.direction = m.ValueRO.direction;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}