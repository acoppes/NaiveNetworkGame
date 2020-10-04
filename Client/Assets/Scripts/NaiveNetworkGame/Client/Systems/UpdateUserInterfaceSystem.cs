using NaiveNetworkGame.Client.Components;
using Scenes;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public class UpdateUserInterfaceSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<ActivePlayer>();
        }

        protected override void OnUpdate()
        {
            // get ui from shared component, singleton or not...
            if (!HasSingleton<ActivePlayer>())
            {
                return;
            }
            
            var activePlayerEntity = GetSingletonEntity<ActivePlayer>();
            var player = EntityManager.GetComponentData<LocalPlayerController>(activePlayerEntity);
            
            Entities.ForEach(delegate(Entity entity, UserInterfaceSharedComponent ui)
            {
                var userInterface = ui.userInterface;
                
                if (userInterface.goldLabel != null)
                    userInterface.goldLabel.SetNumber(player.gold);

                if (userInterface.playerStats)
                {
                    userInterface.playerStats.currentUnits = player.currentUnits;
                    userInterface.playerStats.maxUnits = player.maxUnits;
                    userInterface.playerStats.unitType = player.skinType;
                }
            
                // TODO: use unit type for farm and skin for unit in ui too..
                
                if (userInterface.buildUnitButton != null)
                    userInterface.buildUnitButton.unitType = player.skinType;
                
                if (userInterface.buildFarmButton != null)
                    userInterface.buildFarmButton.unitType = player.skinType;
            });
        }
    }
}