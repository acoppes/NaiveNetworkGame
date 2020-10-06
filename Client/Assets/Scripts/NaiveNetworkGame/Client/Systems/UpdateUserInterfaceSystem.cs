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
                    if (userInterface.buildUnitButton != null && userInterface.buildUnitButton.actionType.unitType == action.type)
                    {
                        userInterface.buildUnitButton.skinType = player.skinType;
                        userInterface.buildUnitButton.cost = action.cost;
                    }
                
                    if (userInterface.buildFarmButton != null && userInterface.buildFarmButton.actionType.unitType == action.type)
                    {
                        userInterface.buildFarmButton.skinType = player.skinType;
                        userInterface.buildFarmButton.cost = action.cost;
                    }    
                }
                
                            
            });
        }
    }
}