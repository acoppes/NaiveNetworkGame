using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateBefore(typeof(DeathOnNoHealthUnitSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial class DamageSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Entities.WithAll<Damage>()
            //     .ForEach(delegate(Entity e, ref Damage d)
            //     {
            //         if (EntityManager.Exists(d.target) && EntityManager.HasComponent<Health>(d.target))
            //         {
            //             var health = EntityManager.GetComponentData<Health>(d.target);
            //             health.current -= d.damage;
            //             PostUpdateCommands.SetComponent(d.target, health);
            //         }
            //         PostUpdateCommands.DestroyEntity(e);
            //     });
            
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            foreach (var (d, e) in SystemAPI.Query<Damage>()
                         .WithEntityAccess())
            {
                if (EntityManager.Exists(d.target) && EntityManager.HasComponent<Health>(d.target))
                {
                    var health = EntityManager.GetComponentData<Health>(d.target);
                    health.current -= d.damage;
                    ecb.SetComponent(d.target, health);
                }
                ecb.DestroyEntity(e);
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}