using NaiveNetworkGame.Client.Components;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    public class SwitchLocalActivePlayerDebugSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            if (!Input.GetKeyUp(KeyCode.Tab))
                return;
            
            if (Entities.WithAll<LocalPlayerController>().ToEntityQuery().CalculateEntityCount() == 1)
                return;

            var activePlayerEntity = Entity.Null;
            Entities
                .WithAll<ActivePlayer, LocalPlayerController>()
                .ForEach(delegate(Entity e)
                {
                    activePlayerEntity = e;
                    PostUpdateCommands.RemoveComponent<ActivePlayer>(e);
                });
            
            // it only works for two player entities

            var switched = false;
            Entities
                .WithAll<LocalPlayerController>()
                .WithNone<ActivePlayer>()
                .ForEach(delegate(Entity e)
                {
                    if (e == activePlayerEntity || switched)
                        return;
                    PostUpdateCommands.AddComponent<ActivePlayer>(e);
                    switched = true;
                });
        }
    }
}