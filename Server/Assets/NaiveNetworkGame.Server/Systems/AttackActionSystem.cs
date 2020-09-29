using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    public class AttackActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities
                .WithAll<Attack, AttackAction, AttackTarget>()
                .ForEach(delegate(Entity e, ref Attack a, ref AttackTarget target, ref AttackAction action)
                {
                    action.time += dt;

                    if (!action.performed && action.time > a.attackTime)
                    {
                        action.performed = true;
                        // do damage and remove...
                        if (EntityManager.Exists(target.target))
                        {
                            var health = EntityManager.GetComponentData<Health>(target.target);
                            health.current -= a.damage;
                            PostUpdateCommands.SetComponent(target.target, health);
                        }
                    }

                    if (action.time > a.duration)
                    {
                        PostUpdateCommands.RemoveComponent<AttackAction>(e);
                        PostUpdateCommands.AddComponent(e, new ReloadAction
                        {
                            time = UnityEngine.Random.Range(-a.reloadRandom, a.reloadRandom)
                        });
                    }
                });
            
            Entities
                .WithAll<Attack, ReloadAction>()
                .ForEach(delegate(Entity e, ref Attack a, ref AttackTarget target, ref ReloadAction action)
                {
                    action.time += dt;
                    if (action.time > a.reload)
                    {
                        PostUpdateCommands.RemoveComponent<ReloadAction>(e);
                    }
                });

            Entities.WithAll<LookingDirection, AttackAction, AttackTarget>()
                .ForEach(delegate(Entity e, ref LookingDirection d, ref Translation t, ref AttackTarget a)
                {
                    if (EntityManager.Exists(a.target))
                    {
                        var targetTranslation = EntityManager.GetComponentData<Translation>(a.target);
                        d.direction = targetTranslation.Value.xy - t.Value.xy;
                    }
                });

        }
    }
}