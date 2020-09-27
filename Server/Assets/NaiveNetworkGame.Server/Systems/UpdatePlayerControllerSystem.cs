using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ProcessPendingPlayerActionsSystem))]
    public class UpdatePlayerControllerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // we could update gold here too...
            
            Entities.ForEach(delegate(ref PlayerController p)
            {
                var player = p.player;

                byte currentUnits = 0;
                
                Entities.ForEach(delegate(ref Unit unit)
                {
                    if (unit.player == player)
                    {
                        currentUnits++;
                    }
                });

                p.currentUnits = currentUnits;
            });
        }
    }
}