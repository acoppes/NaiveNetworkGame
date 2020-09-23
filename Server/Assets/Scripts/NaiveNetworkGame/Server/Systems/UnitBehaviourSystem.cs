using NaiveNetworkGame.Server.Components;
using Server;
using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Systems
{
    // Just a small default wander (around 0,0) behaviour for units...

    public class UnitBehaviourSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // for each unit, calculate best target
            // order by number of attacking units and select it

            // if unit has attack target and not spawning, idle, etc
            // move to attack target...

            // if unit is in attack position
            // perform attack, wait, attack, wait...
            
            Entities
                .WithAll<Unit, Movement, UnitBehaviour>()
                .WithNone<MovementAction, SpawningAction, IdleAction>()
                .ForEach(delegate (Entity e, ref UnitBehaviour behaviour)
                {
                    var offset = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, behaviour.range);
                    PostUpdateCommands.AddComponent(e, new MovementAction
                    {
                        target = behaviour.wanderCenter + new float2(offset.x, offset.y)
                    });
                    PostUpdateCommands.AddComponent(e, new IdleAction
                    {
                        time = UnityEngine.Random.Range(1.0f, 3.0f)
                    });
                });
        }
    }
}