using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct DeathOnNoHealthUnitSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (health, entity) in 
                SystemAPI.Query<RefRO<Health>>()
                    .WithAll<IsAlive, UnitBehaviourComponent>()
                    .WithEntityAccess())
            {
                if (health.ValueRO.current <= 0.01f)
                {
                    // TODO: configure death action with unit behaviour or unit data, or new component death
                    ecb.AddComponent(entity, new DeathAction
                    {
                        time = 0, 
                        duration = 1
                    });
                    ecb.RemoveComponent<IsAlive>(entity);
                    // state.EntityManager.DestroyEntity(entity);
                }
            }
            
            foreach (var (health, entity) in 
                SystemAPI.Query<RefRO<Health>>()
                    .WithNone<UnitBehaviourComponent>()
                    .WithAll<IsAlive>()
                    .WithEntityAccess())
            {
                if (health.ValueRO.current <= 0.01f)
                {
                    ecb.RemoveComponent<IsAlive>(entity);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
