using Client;
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

            var player = GetSingleton<PlayerController>();
            
            // set the ui.gold from player gold...
            
            // update the player input state given the ui state too?
        }
    }
}