using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateBefore(typeof(DamageSystem))]
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

                        var damageEntity = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(damageEntity, new Damage()
                        {
                            target = target.target,
                            damage = a.damage
                        });
                    }

                    if (action.time > a.duration)
                    {
                        PostUpdateCommands.RemoveComponent<AttackAction>(e);
                        PostUpdateCommands.AddComponent(e, new ReloadAction
                        {
                            time = UnityEngine.Random.Range(-a.reloadRandom, a.reloadRandom),
                            duration = a.reload
                        });
                    }
                });
            
            Entities
                .WithAll<Attack, ReloadAction>()
                .ForEach(delegate(Entity e, ref ReloadAction action)
                {
                    action.time += dt;
                    if (action.time > action.duration)
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