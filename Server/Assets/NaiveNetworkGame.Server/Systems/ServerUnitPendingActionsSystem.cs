using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    // Processes pending player move actions to enqueue MoveActions in units (not used anymore)

    public class ServerUnitPendingActionsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<PendingAction, Movement>().WithNone<MovementAction>().ForEach(delegate (Entity e, ref PendingAction p)
            {
                PostUpdateCommands.RemoveComponent<PendingAction>(e);
                
                // movement command
                if (p.command == ClientPlayerAction.MoveUnitAction)
                {
                    PostUpdateCommands.AddComponent(e, new MovementAction
                    {
                        target = p.target
                    });
                }
            });
        }
    }
}