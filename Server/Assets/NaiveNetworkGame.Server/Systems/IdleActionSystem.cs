using NaiveNetworkGame.Server.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    [BurstCompile]
    public partial struct IdleActionSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (_, e) in SystemAPI.Query<IdleAction>()
                         .WithAll<AttackAction>()
                         .WithEntityAccess())
            {
                ecb.RemoveComponent<IdleAction>(e);
            }
            
            foreach (var (idle, e) in SystemAPI.Query<RefRW<IdleAction>>()
                         .WithNone<MovementAction>()
                         .WithNone<SpawningAction>()
                         .WithNone<AttackAction>()
                         .WithNone<DeathAction>()
                         .WithEntityAccess())
            {
                idle.ValueRW.time -= dt;
                if (idle.ValueRW.time < 0)
                {
                    ecb.RemoveComponent<IdleAction>(e);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}