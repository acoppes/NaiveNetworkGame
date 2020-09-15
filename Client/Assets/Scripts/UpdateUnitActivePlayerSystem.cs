using Client;
using Unity.Entities;

namespace Scenes
{
    public class UpdateUnitActivePlayerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // NetworkPlayerId
            
            Entities.ForEach(delegate(ref NetworkPlayerId networkPlayer)
            {
                var player = networkPlayer.player;
                
                Entities.ForEach(delegate(ref Unit unit)
                {
                    unit.isActivePlayer = unit.player == player;
                });
                
            });
        }
    }
}