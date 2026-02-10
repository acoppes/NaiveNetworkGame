using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Systems
{
    // Given actions being processed by unit, updates its state.
    
    [UpdateBefore(typeof(UpdateNetworkGameStateSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct UnitStateSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var unitState in 
                SystemAPI.Query<RefRW<UnitStateComponent>>()
                    .WithNone<MovementAction, SpawningAction, AttackAction>()
                    .WithAll<ServerOnly, IsAlive>())
            {
                unitState.ValueRW.state = UnitStateTypes.idleState;
            }
            
            foreach (var (unitState, spawning) in 
                SystemAPI.Query<RefRW<UnitStateComponent>, RefRO<SpawningAction>>()
                    .WithNone<MovementAction>()
                    .WithAll<ServerOnly, IsAlive>())
            {
                unitState.ValueRW.state = UnitStateTypes.spawningState;
                unitState.ValueRW.percentage = (byte) Mathf.RoundToInt(100.0f * spawning.ValueRO.time / spawning.ValueRO.duration);
            }
            
            foreach (var unitState in 
                SystemAPI.Query<RefRW<UnitStateComponent>>()
                    .WithNone<SpawningAction>()
                    .WithAll<ServerOnly, MovementAction, IsAlive>())
            {
                unitState.ValueRW.state = UnitStateTypes.walkState;
            }

            foreach (var unitState in 
                SystemAPI.Query<RefRW<UnitStateComponent>>()
                    .WithAll<ServerOnly, AttackAction, IsAlive>())
            {
                unitState.ValueRW.state = UnitStateTypes.attackingState;
            }
            
            foreach (var (unitState, reloadAction) in 
                SystemAPI.Query<RefRW<UnitStateComponent>, RefRO<ReloadAction>>()
                    .WithAll<ServerOnly, IsAlive>())
            {
                unitState.ValueRW.state = UnitStateTypes.reloadingState;
                unitState.ValueRW.percentage = (byte) Mathf.RoundToInt(100.0f * reloadAction.ValueRO.time / reloadAction.ValueRO.duration);
            }
            
            foreach (var (unitState, deathAction) in 
                SystemAPI.Query<RefRW<UnitStateComponent>, RefRO<DeathAction>>()
                    .WithNone<IsAlive>()
                    .WithAll<ServerOnly>())
            {
                unitState.ValueRW.state = UnitStateTypes.deathState;
                unitState.ValueRW.percentage = (byte) Mathf.RoundToInt(100.0f * deathAction.ValueRO.time / deathAction.ValueRO.duration);
            }
        }
    }
}