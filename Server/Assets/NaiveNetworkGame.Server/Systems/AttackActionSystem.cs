using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateBefore(typeof(DamageSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class AttackActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities
                .WithAll<AttackComponent, AttackTargetComponent, IsAlive>()
                .WithNone<AttackAction, ReloadAction>()
                .ForEach(delegate(Entity e, ref AttackComponent attack, ref AttackTargetComponent target, ref Translation t)
                {
                    // if target entity was destroyed, forget about it...
                    if (!EntityManager.Exists(target.target))
                    {
                        PostUpdateCommands.RemoveComponent<AttackTargetComponent>(e);
                    }
                    else
                    {
                        // if target too far away, forget about it...
                        var tp = EntityManager.GetComponentData<Translation>(target.target);
                        var isAlive = EntityManager.HasComponent<IsAlive>(target.target);
                        
                        if (!isAlive || math.distancesq(tp.Value, t.Value) > attack.range * attack.range)
                        {
                            PostUpdateCommands.RemoveComponent<AttackTargetComponent>(e);
                        }
                        else
                        {
                            // if lost isalive, lose target...
                            PostUpdateCommands.AddComponent(e, new AttackAction());
                        }
                    } 
                });
            
            Entities.WithAll<AttackAction, DeathAction>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.RemoveComponent<AttackAction>(e);
                });
            
            Entities.WithAll<AttackAction, DisableAttackComponent>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.RemoveComponent<AttackAction>(e);
                });
            
            Entities
                .WithAll<AttackComponent, AttackTargetComponent, AttackAction, MovementAction>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.RemoveComponent<MovementAction>(e);
                });
            
            Entities
                .WithAll<AttackComponent, AttackAction, AttackTargetComponent>()
                .ForEach(delegate(Entity e, ref AttackComponent a, ref AttackTargetComponent target, ref AttackAction action)
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
                .WithAll<AttackComponent, ReloadAction>()
                .ForEach(delegate(Entity e, ref ReloadAction action)
                {
                    action.time += dt;
                    if (action.time > action.duration)
                    {
                        PostUpdateCommands.RemoveComponent<ReloadAction>(e);
                    }
                });

            Entities.WithAll<LookingDirection, AttackAction, AttackTargetComponent>()
                .ForEach(delegate(Entity e, ref LookingDirection d, ref Translation t, ref AttackTargetComponent a)
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