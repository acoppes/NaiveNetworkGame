using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ProcessBuildUnitAction))]
    public class UpdatePlayerUnitSkinSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(delegate(ref PlayerControllerComponentData p)
            {
                var player = p.player;
                var skinType = p.skinType;
                
                // This system only allows one skin per player (can't combine)
                
                Entities
                    .WithAll<Unit, Skin>()
                    .ForEach(delegate(ref Unit u, ref Skin skin)
                    {
                        if (u.player == player)
                        {
                            skin.type = skinType;
                        }
                    });
            });
        }
    }
}