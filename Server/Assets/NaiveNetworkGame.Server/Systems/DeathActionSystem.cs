using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct DeathActionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (_, e) in SystemAPI.Query<MovementAction>()
                         .WithAll<DeathAction>()
                         .WithEntityAccess())
            {
                ecb.RemoveComponent<MovementAction>(e);
            }
            
            // Entities
            //     .WithAll<DeathAction, MovementAction>()
            //     .ForEach((Entity e, ref DeathAction a) =>
            //     {
            //         PostUpdateCommands.RemoveComponent<MovementAction>(e);
            //     }).Run();
            
            foreach (var (_, e) in SystemAPI.Query<ChaseTargetComponent>()
                         .WithAll<DeathAction>()
                         .WithEntityAccess())
            {
                ecb.RemoveComponent<ChaseTargetComponent>(e);
            }
            
            // Entities
            //     .WithAll<DeathAction, ChaseTargetComponent>()
            //     .ForEach((Entity e, ref DeathAction a) =>
            //     {
            //         PostUpdateCommands.RemoveComponent<ChaseTargetComponent>(e);
            //     }).Run();
            
            foreach (var (a, e) in SystemAPI.Query<RefRW<DeathAction>>()
                         .WithNone<IsAlive>()
                         .WithAll<DeathAction>()
                         .WithEntityAccess())
            {
                a.ValueRW.time += dt;
                if (a.ValueRW.time > a.ValueRW.duration)
                {
                    // set completely death?
                    ecb.RemoveComponent<DeathAction>(e);

                    // TODO: for now we are sending gamestate of death units too to keep corpses in client...
                    //PostUpdateCommands.RemoveComponent<NetworkTranslationSync>(e);
                    ecb.RemoveComponent<NetworkGameState>(e);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            // Entities
            //     .WithNone<IsAlive>()
            //     .WithAll<DeathAction>()
            //     .ForEach((Entity e, ref DeathAction a) =>
            //     {
            //         a.time += dt;
            //         if (a.time > a.duration)
            //         {
            //             // set completely death?
            //             PostUpdateCommands.RemoveComponent<DeathAction>(e);
            //
            //             // TODO: for now we are sending gamestate of death units too to keep corpses in client...
            //             //PostUpdateCommands.RemoveComponent<NetworkTranslationSync>(e);
            //             PostUpdateCommands.RemoveComponent<NetworkGameState>(e);
            //         }
            //     }).Run();
        }
    }
}