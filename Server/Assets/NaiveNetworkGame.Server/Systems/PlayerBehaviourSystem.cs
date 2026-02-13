using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    [UpdateBefore(typeof(PlayerBehaviourSystem))]
    public partial struct UpdateChaseCenterSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // By default, we use the unit position as the chase center
            foreach (var (attack, localTransform) in 
                SystemAPI.Query<RefRW<AttackComponent>, RefRO<LocalTransform>>()
                    .WithAll<IsAlive>())
            {
                attack.ValueRW.chaseCenter = localTransform.ValueRO.Position;
            }
        }
    }
    
    [UpdateAfter(typeof(ProcessPendingPlayerActionsSystem))]
    [UpdateBefore(typeof(UnitBehaviourSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct PlayerBehaviourSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (switchToAttack, playerController, behaviour, entity) in 
                SystemAPI.Query<RefRO<SwitchToAttackAction>, RefRO<PlayerController>, RefRW<PlayerBehaviour>>()
                    .WithAll<ServerOnly>()
                    .WithEntityAccess())
            {
                ecb.RemoveComponent<SwitchToAttackAction>(entity);
                
                var player = playerController.ValueRO.player;
                var area = playerController.ValueRO.attackArea;

                behaviour.ValueRW.mode = PlayerBehaviour.aggressive;

                // Remove IdleAction from player's units
                var idleQuery = SystemAPI.QueryBuilder()
                    .WithAll<Unit, IdleAction>()
                    .Build();
                
                foreach (var (unit, unitEntity) in 
                    SystemAPI.Query<RefRO<Unit>>()
                        .WithAll<IdleAction>()
                        .WithEntityAccess())
                {
                    if (unit.ValueRO.player != player)
                        continue;
                    ecb.RemoveComponent<IdleAction>(unitEntity);
                }
                
                // Remove MovementAction from player's units
                foreach (var (unit, unitEntity) in 
                    SystemAPI.Query<RefRO<Unit>>()
                        .WithAll<MovementAction>()
                        .WithEntityAccess())
                {
                    if (unit.ValueRO.player != player)
                        continue;
                    ecb.RemoveComponent<MovementAction>(unitEntity);
                }
            }
            
            foreach (var (switchToDefend, playerController, behaviour, entity) in 
                SystemAPI.Query<RefRO<SwitchToDefendAction>, RefRO<PlayerController>, RefRW<PlayerBehaviour>>()
                    .WithAll<ServerOnly>()
                    .WithEntityAccess())
            {
                ecb.RemoveComponent<SwitchToDefendAction>(entity);
                
                var player = playerController.ValueRO.player;
                var area = playerController.ValueRO.defendArea;

                behaviour.ValueRW.mode = PlayerBehaviour.defensive;

                // Remove IdleAction from player's units
                foreach (var (unit, unitEntity) in 
                    SystemAPI.Query<RefRO<Unit>>()
                        .WithAll<IdleAction>()
                        .WithEntityAccess())
                {
                    if (unit.ValueRO.player != player)
                        continue;
                    ecb.RemoveComponent<IdleAction>(unitEntity);
                }
                
                // Remove MovementAction from player's units
                foreach (var (unit, unitEntity) in 
                    SystemAPI.Query<RefRO<Unit>>()
                        .WithAll<MovementAction>()
                        .WithEntityAccess())
                {
                    if (unit.ValueRO.player != player)
                        continue;
                    ecb.RemoveComponent<MovementAction>(unitEntity);
                }
            }
            
            foreach (var (playerController, localTransform, behaviour) in 
                SystemAPI.Query<RefRO<PlayerController>, RefRO<LocalTransform>, RefRO<PlayerBehaviour>>()
                    .WithAll<ServerOnly>())
            {
                var player = playerController.ValueRO.player;
                var defendCenter = localTransform.ValueRO.Position;
                var defendRange = playerController.ValueRO.defensiveRange * playerController.ValueRO.defensiveRange;

                var attackArea = playerController.ValueRO.attackArea;
                var defendArea = playerController.ValueRO.defendArea;

                switch (behaviour.ValueRO.mode)
                {
                    case PlayerBehaviour.aggressive:
                        
                        foreach (var (unit, unitBehaviour) in 
                            SystemAPI.Query<RefRO<Unit>, RefRW<UnitBehaviourComponent>>())
                        {
                            if (unit.ValueRO.player != player)
                                continue;
                            unitBehaviour.ValueRW.wanderArea = attackArea;
                        }
                        
                        foreach (var (unit, unitEntity) in 
                            SystemAPI.Query<RefRO<Unit>>()
                                .WithAll<DisableAttackComponent>()
                                .WithEntityAccess())
                        {
                            if (unit.ValueRO.player != player)
                                continue;
                            ecb.RemoveComponent<DisableAttackComponent>(unitEntity);
                        }
                        break;
                        
                    case PlayerBehaviour.defensive:
                        // could be processed all the time, not only here...
                        
                        foreach (var (unit, unitBehaviour) in 
                            SystemAPI.Query<RefRO<Unit>, RefRW<UnitBehaviourComponent>>())
                        {
                            if (unit.ValueRO.player != player)
                                continue;
                            unitBehaviour.ValueRW.wanderArea = defendArea;
                        }
                        
                        // While we are on defensive mode, we use the defend center as the center of chase target
                        foreach (var (unit, attack) in 
                            SystemAPI.Query<RefRO<Unit>, RefRW<AttackComponent>>())
                        {
                            if (unit.ValueRO.player != player)
                                continue;
                            attack.ValueRW.chaseCenter = defendCenter;
                        }
                        
                        foreach (var (unit, unitTransform, unitEntity) in 
                            SystemAPI.Query<RefRO<Unit>, RefRO<LocalTransform>>()
                                .WithNone<DisableAttackComponent>()
                                .WithEntityAccess())
                        {
                            if (unit.ValueRO.player != player)
                                continue;

                            if (math.distancesq(unitTransform.ValueRO.Position, defendCenter) > defendRange)
                            {
                                ecb.AddComponent<DisableAttackComponent>(unitEntity);
                            }
                        }
                        
                        foreach (var (unit, unitTransform, unitEntity) in 
                            SystemAPI.Query<RefRO<Unit>, RefRO<LocalTransform>>()
                                .WithAll<DisableAttackComponent>()
                                .WithEntityAccess())
                        {
                            if (unit.ValueRO.player != player)
                                continue;

                            if (math.distancesq(unitTransform.ValueRO.Position, defendCenter) < defendRange)
                            {
                                ecb.RemoveComponent<DisableAttackComponent>(unitEntity);
                            }
                        }
                        
                        // Bulk remove components using QueryBuilder
                        var chaseMovementQuery = SystemAPI.QueryBuilder()
                            .WithAll<ChaseTargetComponent, MovementAction, DisableAttackComponent>()
                            .Build();
                        state.EntityManager.RemoveComponent<MovementAction>(chaseMovementQuery);
                        
                        var chaseTargetQuery = SystemAPI.QueryBuilder()
                            .WithAll<ChaseTargetComponent, DisableAttackComponent>()
                            .Build();
                        state.EntityManager.RemoveComponent<ChaseTargetComponent>(chaseTargetQuery);

                        var attackTargetQuery = SystemAPI.QueryBuilder()
                            .WithAll<AttackTargetComponent, DisableAttackComponent>()
                            .Build();
                        state.EntityManager.RemoveComponent<AttackTargetComponent>(attackTargetQuery);
                        
                        break;
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            // Enable attack again for units in defend area...
        }
    }
}