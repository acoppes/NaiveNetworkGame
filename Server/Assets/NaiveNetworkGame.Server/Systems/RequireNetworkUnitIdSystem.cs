using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public partial struct RequireNetworkUnitIdSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (unit, e) in 
                     SystemAPI.Query<RefRW<Unit>>()
                         .WithAll<RequireNetworkUnitId>()
                         .WithEntityAccess())
            {
                unit.ValueRW.id = NetworkUnitId.current++;
                ecb.RemoveComponent<RequireNetworkUnitId>(e);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}