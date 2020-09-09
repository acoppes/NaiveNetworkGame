using Unity.Entities;

namespace Server
{
    public class DestroyPlayerControllerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<PlayerController, PlayerConnectionId>()
                .ForEach(delegate(Entity playerEntity, ref PlayerConnectionId p)
            {
                if (!p.destroyed)
                    return;

                var player = p.player;
                
                // destroy all player entities
                Entities.ForEach(delegate(Entity unitEntity, ref Unit unit)
                {
                    if (unit.player == player)
                    {
                        PostUpdateCommands.DestroyEntity(unitEntity);
                    }
                });
                
                // destroy player controller
                PostUpdateCommands.DestroyEntity(playerEntity);
            });
        }
    }
}