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
                .WithNone<MovementAction, SpawningAction, AttackAction>()
                .WithAll<ServerOnly, UnitState, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = UnitState.idleState;
                });
            
            Entities
                .WithNone<MovementAction>()
                .WithAll<ServerOnly, SpawningAction, UnitState, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitState u, ref SpawningAction s)
                {
                    u.state = UnitState.spawningState;
                    u.percentage = (byte) Mathf.RoundToInt(100.0f * s.time / s.duration);
                });
            
            Entities
                .WithNone<SpawningAction>()
                .WithAll<ServerOnly, MovementAction, UnitState, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = UnitState.walkState;
                });

            Entities
                .WithAll<ServerOnly, UnitState, AttackAction, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = UnitState.attackingState;
                });
            
            Entities
                .WithAll<ServerOnly, UnitState, ReloadAction, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitState u, ref ReloadAction a)
                {
                    u.state = UnitState.reloadingState;
                    u.percentage = (byte) Mathf.RoundToInt(100.0f * a.time / a.duration);
                });
            
            Entities
                .WithNone<IsAlive>()
                .WithAll<ServerOnly, UnitState, DeathAction>()
                .ForEach(delegate(Entity e, ref UnitState u)
                {
                    u.state = UnitState.deathState;
                });
        }
    }
}