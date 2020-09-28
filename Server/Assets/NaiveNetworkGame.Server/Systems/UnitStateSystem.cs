using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Systems
{
    // Given actions being processed by unit, updates its state.
    
    // [UpdateBefore(typeof(ServerMovementSystem))]
    public class UnitStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<MovementAction>()
                .WithAll<ServerOnly, SpawningAction, UnitState>()
                .ForEach(delegate(Entity e, ref UnitState u, ref SpawningAction s)
                {
                    u.state = UnitState.spawningState;
                    u.percentage = (byte) Mathf.RoundToInt(100.0f * s.time / s.duration);
                });
            
            Entities
                .WithNone<SpawningAction>()
                .WithAll<ServerOnly, MovementAction, UnitState>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = UnitState.walkState;
                });
            
            Entities
                .WithNone<MovementAction, SpawningAction>()
                .WithAll<ServerOnly, UnitState>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = UnitState.idleState;
                });
            
            Entities
                .WithAll<ServerOnly, UnitState, AttackAction>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = UnitState.attackingState;
                });
        }
    }
}