using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Systems
{
    // Given actions being processed by unit, updates its state.
    
    [UpdateBefore(typeof(UpdateNetworkGameStateSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class UnitStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<MovementAction, SpawningAction, AttackAction>()
                .WithAll<ServerOnly, UnitStateComponent, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitStateComponent u)
                {
                    u.state = UnitStateTypes.idleState;
                });
            
            Entities
                .WithNone<MovementAction>()
                .WithAll<ServerOnly, SpawningAction, UnitStateComponent, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitStateComponent u, ref SpawningAction s)
                {
                    u.state = UnitStateTypes.spawningState;
                    u.percentage = (byte) Mathf.RoundToInt(100.0f * s.time / s.duration);
                });
            
            Entities
                .WithNone<SpawningAction>()
                .WithAll<ServerOnly, MovementAction, UnitStateComponent, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitStateComponent u)
                {
                    u.state = UnitStateTypes.walkState;
                });

            Entities
                .WithAll<ServerOnly, UnitStateComponent, AttackAction, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitStateComponent u)
                {
                    u.state = UnitStateTypes.attackingState;
                });
            
            Entities
                .WithAll<ServerOnly, UnitStateComponent, ReloadAction, IsAlive>()
                .ForEach(delegate(Entity e, ref UnitStateComponent u, ref ReloadAction a)
                {
                    u.state = UnitStateTypes.reloadingState;
                    u.percentage = (byte) Mathf.RoundToInt(100.0f * a.time / a.duration);
                });
            
            Entities
                .WithNone<IsAlive>()
                .WithAll<ServerOnly, UnitStateComponent, DeathAction>()
                .ForEach(delegate(Entity e, ref UnitStateComponent u, ref DeathAction a)
                {
                    u.state = UnitStateTypes.deathState;
                    u.percentage = (byte) Mathf.RoundToInt(100.0f * a.time / a.duration);
                });
        }
    }
}