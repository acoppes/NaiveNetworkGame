using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateBefore(typeof(DamageSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct AttackActionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            
            foreach (var (attack, target, localTransform, entity) in 
                SystemAPI.Query<RefRO<AttackComponent>, RefRO<AttackTargetComponent>, RefRO<LocalTransform>>()
                    .WithAll<IsAlive>()
                    .WithNone<AttackAction, ReloadAction>()
                    .WithEntityAccess())
            {
                // if target entity was destroyed, forget about it...
                if (!state.EntityManager.Exists(target.ValueRO.target))
                {
                    state.EntityManager.RemoveComponent<AttackTargetComponent>(entity);
                }
                else
                {
                    // if target too far away, forget about it...
                    var tp = state.EntityManager.GetComponentData<LocalTransform>(target.ValueRO.target);
                    var isAlive = state.EntityManager.HasComponent<IsAlive>(target.ValueRO.target);
                    
                    if (!isAlive || math.distancesq(tp.Position, localTransform.ValueRO.Position) > attack.ValueRO.range * attack.ValueRO.range)
                    {
                        state.EntityManager.RemoveComponent<AttackTargetComponent>(entity);
                    }
                    else
                    {
                        // if lost isalive, lose target...
                        state.EntityManager.AddComponent<AttackAction>(entity);
                    }
                } 
            }
            
            // Remove AttackAction from entities with DeathAction
            var deathQuery = SystemAPI.QueryBuilder().WithAll<AttackAction, DeathAction>().Build();
            state.EntityManager.RemoveComponent<AttackAction>(deathQuery);
            
            // Remove AttackAction from entities with DisableAttackComponent
            var disableQuery = SystemAPI.QueryBuilder().WithAll<AttackAction, DisableAttackComponent>().Build();
            state.EntityManager.RemoveComponent<AttackAction>(disableQuery);
            
            // Remove MovementAction from entities attacking
            var movementQuery = SystemAPI.QueryBuilder().WithAll<AttackComponent, AttackTargetComponent, AttackAction, MovementAction>().Build();
            state.EntityManager.RemoveComponent<MovementAction>(movementQuery);
            
            foreach (var (attack, target, action, entity) in 
                SystemAPI.Query<RefRO<AttackComponent>, RefRO<AttackTargetComponent>, RefRW<AttackAction>>()
                    .WithEntityAccess())
            {
                action.ValueRW.time += dt;

                if (!action.ValueRO.performed && action.ValueRO.time > attack.ValueRO.attackTime)
                {
                    action.ValueRW.performed = true;
                    // do damage and remove...

                    var damageEntity = state.EntityManager.CreateEntity();
                    state.EntityManager.AddComponentData(damageEntity, new Damage()
                    {
                        target = target.ValueRO.target,
                        damage = attack.ValueRO.damage
                    });
                }

                if (action.ValueRO.time > attack.ValueRO.duration)
                {
                    state.EntityManager.RemoveComponent<AttackAction>(entity);
                    state.EntityManager.AddComponentData(entity, new ReloadAction
                    {
                        time = UnityEngine.Random.Range(-attack.ValueRO.reloadRandom, attack.ValueRO.reloadRandom),
                        duration = attack.ValueRO.reload
                    });
                }
            }
            
            foreach (var (action, entity) in 
                SystemAPI.Query<RefRW<ReloadAction>>()
                    .WithAll<AttackComponent>()
                    .WithEntityAccess())
            {
                action.ValueRW.time += dt;
                if (action.ValueRO.time > action.ValueRO.duration)
                {
                    state.EntityManager.RemoveComponent<ReloadAction>(entity);
                }
            }

            foreach (var (lookingDirection, localTransform, attackTarget, entity) in 
                SystemAPI.Query<RefRW<LookingDirection>, RefRO<LocalTransform>, RefRO<AttackTargetComponent>>()
                    .WithAll<AttackAction>()
                    .WithEntityAccess())
            {
                if (state.EntityManager.Exists(attackTarget.ValueRO.target))
                {
                    var targetTransform = state.EntityManager.GetComponentData<LocalTransform>(attackTarget.ValueRO.target);
                    lookingDirection.ValueRW.direction = targetTransform.Position.xy - localTransform.ValueRO.Position.xy;
                }
            }
        }
    }
}