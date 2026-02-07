using NaiveNetworkGame.Server.Components;
using Unity.Burst;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    [BurstCompile]
    public partial class IdleActionSystem : SystemBase
    {
        [BurstCompile]
        protected override void OnUpdate()
        {
            var dt = SystemAPI.Time.DeltaTime;

            foreach (var (_, e) in SystemAPI.Query<IdleAction>()
                         .WithAll<AttackAction>()
                         .WithEntityAccess())
            {
                EntityManager.RemoveComponent<IdleAction>(e);
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
                    EntityManager.RemoveComponent<IdleAction>(e);
                }
            }
            
            // EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            //
            // // if for some reason we have an attack action pending... remove idle
            // Entities
            //     .WithAll<IdleAction, AttackAction>()
            //     .ForEach((Entity e) =>
            //     {
            //         ecb.RemoveComponent<IdleAction>(e);
            //         // PostUpdateCommands.RemoveComponent<IdleAction>(e);
            //     }).Run();
            //
            // Entities
            //     .WithNone<MovementAction, SpawningAction, AttackAction>()
            //     .WithNone<DeathAction>()
            //     .WithAll<IdleAction>()
            //     .ForEach((Entity e, ref IdleAction idle) =>
            //     {
            //         idle.time -= dt;
            //
            //         if (idle.time < 0)
            //         {
            //             ecb.RemoveComponent<IdleAction>(e);
            //             // PostUpdateCommands.RemoveComponent<IdleAction>(e);
            //         }
            //     }).Run();
            //
            // ecb.Playback(EntityManager);
            // ecb.Dispose();
        }
    }
}