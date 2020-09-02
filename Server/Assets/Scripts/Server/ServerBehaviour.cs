using Unity.Entities;
using UnityEngine;

namespace Server
{
    public struct PlayerControllerSharedComponent : ISharedComponentData
    {
        public Entity unitPrefab;
        public Entity treePrefab;
    }

    public static class CommandLineArguments
    {
        public static bool HasArgument(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(name))
                {
                    return true;
                }
            }
            return false;
        }
        
        public static string GetArgument(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }

    
    public class ServerBehaviour : MonoBehaviour
    {
        public int targetFrameRate = 60;
        public int unitsPerPlayer = 2;
        
        public GameObject unitPrefab;
        public GameObject treePrefab;
        
        public int totalOutputInBytes;
        public int lastFrameOutputInBytes;

        public double totalOutputInKB;
        public double totalOutputInMB;

        public double bytesPerSecond;
        public double kbPerSecond;
        public double mbPerSecond;
        
        private float consoleLogFrequencyInSeconds = 5;
        private float timeSincelastLog = 0;

        private bool logStatistics = false;
        
        private void Start ()
        {
            ServerNetworkStaticData.startingUnitsPerPlayer = unitsPerPlayer;
            
            // set default port
            ushort port = 9000;
            // default framerate
            Application.targetFrameRate = targetFrameRate;

            var targetFrameRateArgument = CommandLineArguments.GetArgument("-targetFrameRate");
            
            if (!string.IsNullOrEmpty(targetFrameRateArgument))
            {
                if (int.TryParse(targetFrameRateArgument, out var customFrameRate))
                {
                    Debug.Log($"Override framerate with custom value: {customFrameRate}");
                    Application.targetFrameRate = customFrameRate;
                }
            }
            
            var portArgument = CommandLineArguments.GetArgument("-port");

            if (!string.IsNullOrEmpty(portArgument))
            {
                if (ushort.TryParse(portArgument, out var portOverride))
                {
                    Debug.Log($"Override port with custom value: {portOverride}");
                    port = portOverride;
                }
            }
            
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var serverEntity = entityManager.CreateEntity(ComponentType.ReadOnly<ServerOnly>());
            entityManager.AddSharedComponentData(serverEntity, new NetworkManagerSharedComponent());
            entityManager.AddComponentData(serverEntity, new ServerStartComponent
            {
                port = port
            });
            
            // create multiple player controllers, all disabled....
            // with each connection, remove disabled component.

            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            
            var playerControllerPrefabEntity = entityManager.CreateEntity();
            entityManager.AddSharedComponentData(playerControllerPrefabEntity, new PlayerControllerSharedComponent
            {
                unitPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(unitPrefab, settings),
                treePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(treePrefab, settings)
            });

            var createdUnitsEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(createdUnitsEntity, new CreatedUnits()
            {
                lastCreatedUnitId = 0
            });
            
            logStatistics = CommandLineArguments.HasArgument("-logStatistics");
            
            Debug.Log("Starting server instance");
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

            if (logStatistics)
            {
                if (Time.realtimeSinceStartup - timeSincelastLog > consoleLogFrequencyInSeconds)
                {
                    // log stuff...
                    
                    Debug.Log($"Total Output (KB): {totalOutputInKB}");
                    Debug.Log($"Connected Players: {ServerNetworkStatistics.currentConnections}");

                    timeSincelastLog = Time.realtimeSinceStartup;
                }
            }

        }
        
    }
}