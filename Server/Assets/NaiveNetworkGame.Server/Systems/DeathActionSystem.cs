using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial class DeathActionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = SystemAPI.Time.DeltaTime;
            
            foreach (var (_, e) in SystemAPI.Query<MovementAction>()
                         .WithAll<DeathAction>()
                         .WithEntityAccess())
            {
                EntityManager.RemoveComponent<MovementAction>(e);
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
                EntityManager.RemoveComponent<ChaseTargetComponent>(e);
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
                    EntityManager.RemoveComponent<DeathAction>(e);

                    // TODO: for now we are sending gamestate of death units too to keep corpses in client...
                    //PostUpdateCommands.RemoveComponent<NetworkTranslationSync>(e);
                    EntityManager.RemoveComponent<NetworkGameState>(e);
                }
            }

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