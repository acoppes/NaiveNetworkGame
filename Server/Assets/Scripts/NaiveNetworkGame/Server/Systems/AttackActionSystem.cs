using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Mathematics;
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
                    
                    // wait some time (animation)

                    // perform damage

                    // complete animation

                    // add reload action (idle time between attacks)
                    // remove attack action

                    if (action.time > a.duration)
                    {
                        // do damage and remove...
                        var health = EntityManager.GetComponentData<Health>(target.target);
                        health.current -= a.damage;
                        PostUpdateCommands.SetComponent(target.target, health);
                        PostUpdateCommands.RemoveComponent<AttackAction>(e);
                    }
                });

            Entities.WithAll<LookingDirection, AttackAction, AttackTarget>()
                .ForEach(delegate(Entity e, ref LookingDirection d, ref Translation t, ref AttackTarget a)
                {
                    var targetTranslation = EntityManager.GetComponentData<Translation>(a.target);
                    d.direction = targetTranslation.Value.xy - t.Value.xy;
                });

        }
    }
}