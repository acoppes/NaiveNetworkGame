using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public class DestroyDisconnectedPlayerUnitsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<PlayerConnectionId>()
                .WithAll<PlayerController>()
                .ForEach(delegate(Entity playerEntity, ref PlayerController pc)
            {
                var player = pc.player;
                
                // destroy all player units if no connection
                Entities.ForEach(delegate(Entity unitEntity, ref Unit unit)
                {
                    if (unit.player == player)
                    {
                        PostUpdateCommands.DestroyEntity(unitEntity);
                    }
                });
            });
            
            // Stop sending dead units gamestate sync...
            // Entities
            //     .WithNone<IsAlive>()
            //     .WithAll<ServerOnly, NetworkTranslationSync, NetworkGameState>()
            //     .ForEach(delegate(Entity e)
            //     {
            //         PostUpdateCommands.RemoveComponent<NetworkTranslationSync>(e);
            //         PostUpdateCommands.RemoveComponent<NetworkGameState>(e);
            //     });
        }
    }
}