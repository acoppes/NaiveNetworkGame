using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Server.Components;
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

            // player wasny sync...
            if (player.player == 0)
                return;
            
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
                
                var actions = GetBufferFromEntity<PlayerAction>()[activePlayerEntity];

                foreach (var action in actions)
                {
                    var unitButton = userInterface.buildUnitButton;
                    
                    if (unitButton != null && unitButton.actionType.unitType == action.type)
                    {
                        unitButton.skinType = player.skinType;
                        unitButton.cost = action.cost;
                    }

                    var farmButton = userInterface.buildFarmButton;
                    if (farmButton != null && farmButton.actionType.unitType == action.type)
                    {
                        farmButton.skinType = player.skinType;
                        farmButton.cost = action.cost;
                    }

                    unitButton.enabled = unitButton.cost < player.gold;
                    farmButton.enabled = farmButton.cost < player.gold;
                }
                
                            
            });
        }
    }
}