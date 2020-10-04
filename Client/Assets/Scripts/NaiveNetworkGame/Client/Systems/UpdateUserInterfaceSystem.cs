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
                if (ui.goldLabel != null)
                    ui.goldLabel.SetNumber(player.gold);

                if (ui.playerStats)
                {
                    ui.playerStats.currentUnits = player.currentUnits;
                    ui.playerStats.maxUnits = player.maxUnits;
                    ui.playerStats.unitType = player.skinType;
                }
            
                if (ui.spawnUnitButton != null)
                    ui.spawnUnitButton.unitType = player.skinType;
            });
     
            // set the ui.gold from player gold...

            // update the player input state given the ui state too?
        }
    }
}