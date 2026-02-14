using NaiveNetworkGame.Client.Components;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public struct SwitchLocalPlayerAction : IComponentData
    {
        
    }
    
    public partial struct SwitchLocalActivePlayerDebugSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (_, e) in 
                SystemAPI.Query<RefRO<SwitchLocalPlayerAction>>()
                    .WithEntityAccess())
            {
                ecb.DestroyEntity(e);
                
                var playerQuery = SystemAPI.QueryBuilder().WithAll<LocalPlayerController>().Build();
                if (playerQuery.CalculateEntityCount() == 1)
                    continue;

                var activePlayerEntity = Entity.Null;
                foreach (var (_, entity) in 
                    SystemAPI.Query<RefRO<ActivePlayerComponent>>()
                        .WithAll<LocalPlayerController>()
                        .WithEntityAccess())
                {
                    activePlayerEntity = entity;
                    ecb.RemoveComponent<ActivePlayerComponent>(entity);
                }
            
                // it only works for two player entities

                var switched = false;
                foreach (var (_, entity) in 
                    SystemAPI.Query<RefRO<LocalPlayerController>>()
                        .WithNone<ActivePlayerComponent>()
                        .WithEntityAccess())
                {
                    if (entity == activePlayerEntity || switched)
                        continue;
                    ecb.AddComponent<ActivePlayerComponent>(entity);
                    switched = true;
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
