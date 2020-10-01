using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateBefore(typeof(DestroyDeathUnitsSystem))]
    public class DamageSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<Damage>()
                .ForEach(delegate(Entity e, ref Damage d)
                {
                    if (EntityManager.Exists(d.target) && EntityManager.HasComponent<Health>(d.target))
                    {
                        var health = EntityManager.GetComponentData<Health>(d.target);
                        health.current -= d.damage;
                        PostUpdateCommands.SetComponent(d.target, health);
                    }
                    PostUpdateCommands.DestroyEntity(e);
                });
        }
    }
}