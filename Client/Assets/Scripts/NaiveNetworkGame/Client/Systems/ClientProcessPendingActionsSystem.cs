using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Client.Systems
{
    public class ClientProcessPendingActionsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach(delegate(ref NetworkPlayerId networkPlayerId, ref LocalPlayerControllerComponentData p, ref PlayerPendingActionComponent playerPendingActions)
            {
                if (networkPlayerId.state != NetworkConnection.State.Connected)
                    return;

                // if (!networkPlayerId.assigned)
                //     return;
                
                // unselect player logic...
                
                // Entities
                //     .WithAllReadOnly<Selectable>()
                //     .WithAll<Unit>().ForEach(delegate(ref Unit unit)
                //     {
                //         unit.isSelected = false;
                //     });

                if (playerPendingActions.pending)
                {
                    // send player action...
                    var e = PostUpdateCommands.CreateEntity();
                    PostUpdateCommands.AddComponent(e, new PendingPlayerAction
                    {
                        player = p.player,
                        actionType = playerPendingActions.actionType,
                        unitType = playerPendingActions.unitType
                    });

                    playerPendingActions.pending = false;
                }
            });
        }
    }
}