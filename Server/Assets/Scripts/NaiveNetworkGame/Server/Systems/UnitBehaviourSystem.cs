using Server;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    // Just a small default wander (around 0,0) behaviour for units...

    public class UnitBehaviourSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Unit, Movement>()
                .WithNone<MovementAction, SpawningAction, IdleAction>()
                .ForEach(delegate (Entity e)
                {
                    PostUpdateCommands.AddComponent(e, new MovementAction
                    {
                        target = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, 1.25f)
                    });
                    PostUpdateCommands.AddComponent(e, new IdleAction
                    {
                        time = UnityEngine.Random.Range(1.0f, 3.0f)
                    });
                });
        }
    }
}