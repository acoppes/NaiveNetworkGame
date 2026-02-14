using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    // Processes spawn over time logic for units...
    
    [UpdateBefore(typeof(UnitStateSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct SpawningActionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (spawningAction, entity) in 
                SystemAPI.Query<RefRW<SpawningAction>>()
                    .WithEntityAccess())
            {
                spawningAction.ValueRW.time += dt;

                if (spawningAction.ValueRO.time >= spawningAction.ValueRO.duration)
                {
                    ecb.RemoveComponent<SpawningAction>(entity);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
