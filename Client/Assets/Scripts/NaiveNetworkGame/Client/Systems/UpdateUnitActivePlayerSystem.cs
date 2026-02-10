using NaiveNetworkGame.Client.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    // for each unit, it updates if the unit is owned by local player
    public partial class UpdateUnitActivePlayerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(delegate(ref LocalPlayerControllerComponentData p)
            {
                var player = p.player;
                
                Entities.ForEach(delegate(ref Unit unit)
                {
                    unit.isLocalPlayer = unit.player == player;
                });
                
            });
        }
    }
}