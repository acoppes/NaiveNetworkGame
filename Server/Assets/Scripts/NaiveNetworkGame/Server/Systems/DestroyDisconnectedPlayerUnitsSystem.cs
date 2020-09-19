using NaiveNetworkGame.Server.Components;
using Server;
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
        }
    }
}