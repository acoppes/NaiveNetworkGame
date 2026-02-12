using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Server.Components;
using Scenes;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public partial struct UpdateUserInterfaceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ActivePlayerComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // get ui from shared component, singleton or not...
            if (!SystemAPI.HasSingleton<ActivePlayerComponent>())
            {
                return;
            }
            
            var activePlayerEntity = SystemAPI.GetSingletonEntity<ActivePlayerComponent>();
            var player = state.EntityManager.GetComponentData<LocalPlayerControllerComponentData>(activePlayerEntity);

            // player wasn't sync...
            if (player.player == 0)
                return;
            
            foreach (var ui in 
                SystemAPI.Query<UserInterfaceSharedComponent>())
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
                
                var actions = state.EntityManager.GetBuffer<PlayerAction>(activePlayerEntity);

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
            }
        }
    }
}
