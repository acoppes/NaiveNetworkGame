using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;

namespace Server
{
    // public class CreatePlayerControllerSystem : ComponentSystem
    // {
    //     protected override void OnUpdate()
    //     {
    //         var query = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerSpawnPosition>());
    //         var spawnPositions = query.ToComponentDataArray<PlayerSpawnPosition>(Allocator.TempJob);
    //
    //         Entities
    //             .WithNone<PlayerController>()
    //             .ForEach(delegate(Entity e, ref PlayerConnectionId p)
    //         {
    //             PostUpdateCommands.AddComponent(e, new PlayerController
    //             {
    //                 gold = 100
    //             });
    //             PostUpdateCommands.AddComponent(e, new NetworkPlayerState());
    //
    //             // for (var i = 0; i < spawnPositions.Length; i++)
    //             // {
    //             //     var spawnPosition = spawnPositions[i];
    //             //     if (spawnPosition.player == p.player)
    //             //     {
    //             //         PostUpdateCommands.AddComponent(e, spawnPosition);
    //             //     }
    //             // }
    //         });
    //
    //         spawnPositions.Dispose();
    //     }
    // }
}