using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;

namespace Server
{
    public struct PlayerControllerSharedComponent : ISharedComponentData
    {
        public Entity prefab;
    }
    
    public class ServerBehaviour : MonoBehaviour
    {
        public int targetFrameRate = 60;
        public GameObject go;

        public int totalOutputInBytes;
        public int lastFrameOutputInBytes;

        public double totalOutputInKB;
        public double totalOutputInMB;

        public double bytesPerSecond;
        public double kbPerSecond;
        public double mbPerSecond;
        
        private void Start ()
        {
            Application.targetFrameRate = targetFrameRate;
            
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var serverEntity = entityManager.CreateEntity(ComponentType.ReadOnly<ServerOnly>());
            entityManager.AddSharedComponentData(serverEntity, new NetworkManagerSharedComponent());
            entityManager.AddComponentData(serverEntity, new ServerStartComponent());
            
            // create multiple player controllers, all disabled....
            // with each connection, remove disabled component.

            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, settings);

            var playerControllerPrefabEntity = entityManager.CreateEntity();
            entityManager.AddSharedComponentData(playerControllerPrefabEntity, new PlayerControllerSharedComponent
            {
                prefab = prefab
            });
        }

        private void Update()
        {
            totalOutputInBytes = ServerNetworkStatistics.outputBytesTotal;
            lastFrameOutputInBytes = ServerNetworkStatistics.outputBytesLastFrame;

            totalOutputInKB = System.Math.Round(ServerNetworkStatistics.outputBytesTotal / 1024.0f, 3);
            totalOutputInMB = System.Math.Round(totalOutputInKB / 1024.0f, 3);

            bytesPerSecond = System.Math.Round(ServerNetworkStatistics.outputBytesTotal / Time.realtimeSinceStartup, 3);
            kbPerSecond = System.Math.Round(totalOutputInKB / Time.realtimeSinceStartup, 3);
            mbPerSecond = System.Math.Round(totalOutputInMB / Time.realtimeSinceStartup, 3);

        }
        
    }
}