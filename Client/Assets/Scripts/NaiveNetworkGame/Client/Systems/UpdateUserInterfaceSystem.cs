using Scenes;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public class UpdateUserInterfaceSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<PlayerController>();
        }

        protected override void OnUpdate()
        {
            // get ui from shared component, singleton or not...

            var playerEntity = GetSingletonEntity<PlayerController>();
            var player = EntityManager.GetComponentData<PlayerController>(playerEntity);
            var ui = EntityManager.GetSharedComponentData<UserInterfaceComponent>(playerEntity);
            
            
            if (ui.goldLabel != null)
                ui.goldLabel.SetNumber(player.gold);

            if (ui.playerStats)
            {
                ui.playerStats.currentUnits = player.currentUnits;
                ui.playerStats.maxUnits = player.maxUnits;
                ui.playerStats.unitType = player.unitType;
            }
            
            if (ui.spawnUnitButton != null)
                ui.spawnUnitButton.unitType = player.unitType;

            // set the ui.gold from player gold...

            // update the player input state given the ui state too?
        }
    }
}