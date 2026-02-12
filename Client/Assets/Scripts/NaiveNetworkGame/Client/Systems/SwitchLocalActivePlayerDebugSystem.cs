using NaiveNetworkGame.Client.Components;
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
            foreach (var (_, e) in 
                SystemAPI.Query<RefRO<SwitchLocalPlayerAction>>()
                    .WithEntityAccess())
            {
                state.EntityManager.DestroyEntity(e);
                
                var playerQuery = SystemAPI.QueryBuilder().WithAll<LocalPlayerControllerComponentData>().Build();
                if (playerQuery.CalculateEntityCount() == 1)
                    continue;

                var activePlayerEntity = Entity.Null;
                foreach (var (_, entity) in 
                    SystemAPI.Query<RefRO<ActivePlayerComponent>>()
                        .WithAll<LocalPlayerControllerComponentData>()
                        .WithEntityAccess())
                {
                    activePlayerEntity = entity;
                    state.EntityManager.RemoveComponent<ActivePlayerComponent>(e);
                }
            
                // it only works for two player entities

                var switched = false;
                foreach (var (_, entity) in 
                    SystemAPI.Query<RefRO<LocalPlayerControllerComponentData>>()
                        .WithNone<ActivePlayerComponent>()
                        .WithEntityAccess())
                {
                    if (entity == activePlayerEntity || switched)
                        continue;
                    state.EntityManager.AddComponent<ActivePlayerComponent>(entity);
                    switched = true;
                }
            }
        }
    }
}
