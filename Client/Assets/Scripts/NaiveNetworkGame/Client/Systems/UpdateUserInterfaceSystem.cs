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

                var unitButton = userInterface.buildUnitButton;
                var houseButton = userInterface.buildHouseButton;
                var barracksButton = userInterface.buildBarracksButton;

                foreach (var action in actions)
                {
                    if (unitButton != null && unitButton.actionType.unitType == action.type)
                    {
                        unitButton.skinType = player.skinType;
                        unitButton.cost = action.cost;
                    }

                    if (houseButton != null && houseButton.actionType.unitType == action.type)
                    {
                        houseButton.skinType = player.skinType;
                        houseButton.cost = action.cost;
                    }
                    
                    if (barracksButton != null && barracksButton.actionType.unitType == action.type)
                    {
                        barracksButton.skinType = player.skinType;
                        barracksButton.cost = action.cost;
                    }
                }
                
                unitButton.enabled = unitButton.cost <= player.gold && player.currentUnits < player.maxUnits 
                                                                    && player.freeBarracksCount > 0;
                
                houseButton.enabled = houseButton.cost <= player.gold && player.buildingSlots > 0;
                barracksButton.enabled = barracksButton.cost <= player.gold && player.buildingSlots > 0;

                userInterface.attackButton.enabled = player.behaviourMode == 0;
                userInterface.defendButton.enabled = player.behaviourMode == 1;
            });
        }
    }
}