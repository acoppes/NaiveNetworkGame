using NaiveNetworkGame.Client.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public struct SwitchLocalPlayerAction : IComponentData
    {
        
    }
    
    public partial class SwitchLocalActivePlayerDebugSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SwitchLocalPlayerAction>().ForEach(delegate(Entity switchCommand)
            {
                PostUpdateCommands.DestroyEntity(switchCommand);
                
                if (Entities.WithAll<LocalPlayerControllerComponentData>().ToEntityQuery().CalculateEntityCount() == 1)
                    return;

                var activePlayerEntity = Entity.Null;
                Entities
                    .WithAll<ActivePlayerComponent, LocalPlayerControllerComponentData>()
                    .ForEach(delegate(Entity e)
                    {
                        activePlayerEntity = e;
                        PostUpdateCommands.RemoveComponent<ActivePlayerComponent>(e);
                    });
            
                // it only works for two player entities

                var switched = false;
                Entities
                    .WithAll<LocalPlayerControllerComponentData>()
                    .WithNone<ActivePlayerComponent>()
                    .ForEach(delegate(Entity e)
                    {
                        if (e == activePlayerEntity || switched)
                            return;
                        PostUpdateCommands.AddComponent<ActivePlayerComponent>(e);
                        switched = true;
                    });
            });
            
            
        }
    }
}