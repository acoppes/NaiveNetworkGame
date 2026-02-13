using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Client.Systems
{
    public partial struct ClientProcessPendingActisonsSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (networkPlayerId, playerPendingActions, localPlayer) in SystemAPI.Query<RefRO<NetworkPlayerId>, 
                             RefRW<PlayerPendingAction>, 
                             RefRO<LocalPlayerController>>())
            {
                if (networkPlayerId.ValueRO.state != NetworkConnection.State.Connected)
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

                if (playerPendingActions.ValueRW.pending)
                {
                    // send player action...
                    var e = state.EntityManager.CreateEntity();
                    state.EntityManager.AddComponentData(e, new PendingPlayerAction
                    {
                        player = localPlayer.ValueRO.player,
                        actionType = playerPendingActions.ValueRW.actionType,
                        unitType = playerPendingActions.ValueRW.unitType
                    });

                    playerPendingActions.ValueRW.pending = false;
                }
            }
            
            // Entities
            //     .ForEach(delegate(ref NetworkPlayerId networkPlayerId, ref LocalPlayerControllerComponentData p, ref PlayerPendingAction playerPendingActions)
            // {
            //
            // });
        }
    }
}