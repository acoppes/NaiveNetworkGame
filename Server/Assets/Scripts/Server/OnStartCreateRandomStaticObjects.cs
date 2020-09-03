using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Server
{
    public class OnStartCreateRandomStaticObjects : ComponentSystem
    {
        private bool staticObjectsCreated;
        
        protected override void OnUpdate()
        {
            if (staticObjectsCreated)
                return;
            
            staticObjectsCreated = true;
            
            var playerControllerPrefabEntity = 
                Entities.WithAll<PlayerControllerSharedComponent>().ToEntityQuery().GetSingletonEntity();

            var prefabsManager = 
                EntityManager.GetSharedComponentData<PlayerControllerSharedComponent>(playerControllerPrefabEntity);

            var createdUnitsEntity = Entities.WithAll<CreatedUnits>().ToEntityQuery().GetSingletonEntity();
            var createdUnits = EntityManager.GetComponentData<CreatedUnits>(createdUnitsEntity);

            var randomCount = UnityEngine.Random.Range(5, 15);

            for (var i = 0; i < randomCount; i++)
            {
                // TODO: maybe we dont need the unit component?
                // TODO: or we can use player 0 as the server?
                
                var playerControllerEntity = PostUpdateCommands.Instantiate(prefabsManager.treePrefab);
                PostUpdateCommands.SetComponent(playerControllerEntity, new Translation
                {
                    Value = new float3(
                        UnityEngine.Random.Range(-1.5f, 1.5f), 
                        UnityEngine.Random.Range(-1.0f, 1.0f), 
                        0)
                });
                PostUpdateCommands.SetComponent(playerControllerEntity, new Unit
                {
                    id = (uint) createdUnits.lastCreatedUnitId++,
                    player = 0,
                    type = 1
                });
                PostUpdateCommands.AddComponent(playerControllerEntity, new NetworkGameState
                {
                    // syncVersion = -1
                });
            }
            
            PostUpdateCommands.SetComponent(createdUnitsEntity, createdUnits);
        }
    }
}