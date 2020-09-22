using NaiveNetworkGame.Common;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public class UpdateNetworkPlayerStateSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<PlayerController>();
        }
        
        protected override void OnUpdate()
        {
            // given a network gamestate, update player local data
            
            Entities.ForEach(delegate(Entity e, ref NetworkPlayerState p)
            {
                // we normally don't get other players state...
                // var controller = GetSingleton<PlayerController>();
                var controllerEntity = GetSingletonEntity<PlayerController>();

                var controller = EntityManager.GetComponentData<PlayerController>(controllerEntity);
                controller.gold = p.gold;
                controller.player = p.player;
                controller.unitType = p.unitType;
                controller.currentUnits = p.currentUnits;
                controller.maxUnits = p.maxUnits;
                PostUpdateCommands.SetComponent(controllerEntity, controller);
                
                // SetSingleton(controller);
                
                PostUpdateCommands.DestroyEntity(e);
            });
        }
    }
}