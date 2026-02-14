using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(TargetingSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct UnitBehaviourSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            // if we have attack target, remove chase target...
            var removeChaseQuery = SystemAPI.QueryBuilder()
                .WithAll<AttackTargetComponent, ChaseTargetComponent>()
                .Build();
            state.EntityManager.RemoveComponent<ChaseTargetComponent>(removeChaseQuery);
            
            // Stop chasing if target is not valid or is not alive
            foreach (var (chaseTarget, entity) in 
                SystemAPI.Query<RefRO<ChaseTargetComponent>>()
                    .WithEntityAccess())
            {
                if (!state.EntityManager.Exists(chaseTarget.ValueRO.target))
                {
                    ecb.RemoveComponent<ChaseTargetComponent>(entity);
                }
                else
                {
                    var isAlive = state.EntityManager.HasComponent<IsAlive>(chaseTarget.ValueRO.target);
                    if (!isAlive)
                    {
                        ecb.RemoveComponent<ChaseTargetComponent>(entity);
                    }
                } 
            }
        

            foreach (var (chaseTarget, entity) in 
                SystemAPI.Query<RefRO<ChaseTargetComponent>>()
                    .WithNone<ReloadAction, DeathAction, MovementAction>()
                    .WithNone<SpawningAction, AttackAction, AttackTargetComponent>()
                    .WithAll<Unit, Movement, UnitBehaviourComponent>()
                    .WithAll<IsAlive>()
                    .WithEntityAccess())
            {
                var targetTransform = state.EntityManager.GetComponentData<LocalTransform>(chaseTarget.ValueRO.target);
                
                ecb.AddComponent(entity, new MovementAction
                {
                    target = targetTransform.Position.xy
                });
            }
            
            foreach (var (movementAction, chaseTarget) in 
                SystemAPI.Query<RefRW<MovementAction>, RefRO<ChaseTargetComponent>>()
                    .WithNone<ReloadAction, DeathAction, SpawningAction>()
                    .WithNone<AttackAction, AttackTargetComponent>()
                    .WithAll<Unit, Movement, UnitBehaviourComponent>()
                    .WithAll<IsAlive>())
            {
                var targetTransform = state.EntityManager.GetComponentData<LocalTransform>(chaseTarget.ValueRO.target);
                movementAction.ValueRW.target = targetTransform.Position.xy;
            }
            
            foreach (var (behaviour, entity) in 
                SystemAPI.Query<RefRO<UnitBehaviourComponent>>()
                    .WithNone<ReloadAction, ChaseTargetComponent, DeathAction>()
                    .WithNone<MovementAction, SpawningAction, IdleAction>()
                    .WithNone<AttackAction, AttackTargetComponent>()
                    .WithAll<Unit, Movement, IsAlive>()
                    .WithEntityAccess())
            {
                var wanderAreaEntity = behaviour.ValueRO.wanderArea;
                
                if (state.EntityManager.Exists(wanderAreaEntity))
                {
                    var wanderArea = state.EntityManager.GetComponentData<WanderArea>(wanderAreaEntity);
                    var wanderCenter = state.EntityManager.GetComponentData<LocalTransform>(wanderAreaEntity);
                    
                    var offset = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, wanderArea.range);
                    ecb.AddComponent(entity, new MovementAction
                    {
                        target = wanderCenter.Position.xy + new float2(offset.x, offset.y)
                    });
                    ecb.AddComponent(entity, new IdleAction
                    {
                        time = UnityEngine.Random.Range(behaviour.ValueRO.minIdleTime, behaviour.ValueRO.maxIdleTime)
                    });    
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
