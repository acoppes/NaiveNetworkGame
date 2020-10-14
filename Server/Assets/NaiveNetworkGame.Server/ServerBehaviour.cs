using NaiveNetworkGame.Server.Components;
using NaiveNetworkGame.Server.Systems;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server
{
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

        public float sendTranslationStateFrequency = 0.1f;
        public float sendGameStateFrequency = 0.1f;

        public int totalOutputInBytes;
        public int lastFrameOutputInBytes;

        public int lastSecondOutputInBytes;

        public double totalOutputInKB;
        public double totalOutputInMB;

        public double bytesPerSecond;
        public double kbPerSecond;
        public double mbPerSecond;
        
        private float consoleLogFrequencyInSeconds = 5;
        private float timeSincelastLog = 0;

        private bool logStatistics = false;

        public bool autostartServerSimulation;

        private void Start ()
        {
            ServerNetworkStaticData.sendGameStateFrequency = sendGameStateFrequency;
            ServerNetworkStaticData.sendTranslationStateFrequency = sendTranslationStateFrequency;
            
            // set default port
            ushort port = 9000;
            // default framerate
            if (targetFrameRate > 0)
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

            var startServerCommand = entityManager.CreateEntity();
            entityManager.AddComponentData(startServerCommand, new StartServerCommand
            {
                port = port
            });
            
            // create multiple player controllers, all disabled....
            // with each connection, remove disabled component.

            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            
            // How to convert from GameObject to Entity
            // GameObjectConversionUtility.ConvertGameObjectHierarchy(unitPrefab, settings)

            logStatistics = CommandLineArguments.HasArgument("-logStatistics");
            
            Debug.Log("Starting server instance");

            if (autostartServerSimulation)
            {
                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                manager.CreateEntity(typeof(ServerSimulation));
            }
        }

        // private void OnDestroy()
        // {
        //     // TODO: destroy all entities and singletons...
        //     // var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //     // entityManager.DestroyEntity(entityManager.UniversalQuery);
        //     
        //     // var entities = entityManager.GetAllEntities(Allocator.TempJob);
        //     // for (var i = 0; i < entities.Length; i++)
        //     // {
        //     //     entityManager.DestroyEntity(entities[i]);
        //     // }
        //     // entities.Dispose();
        // }

        private float timeSinceLastSecondUpdate;
        private int previousTotalBytes;
        
        private void Update()
        {
            ServerNetworkStaticData.sendGameStateFrequency = sendGameStateFrequency;
            ServerNetworkStaticData.sendTranslationStateFrequency = sendTranslationStateFrequency;
            
            Application.targetFrameRate = targetFrameRate;
            
            totalOutputInBytes = ServerNetworkStatistics.outputBytesTotal;
            lastFrameOutputInBytes = ServerNetworkStatistics.outputBytesLastFrame;

            totalOutputInKB = System.Math.Round(ServerNetworkStatistics.outputBytesTotal / 1024.0f, 3);
            totalOutputInMB = System.Math.Round(totalOutputInKB / 1024.0f, 3);

            bytesPerSecond = System.Math.Round(ServerNetworkStatistics.outputBytesTotal / Time.realtimeSinceStartup, 3);
            kbPerSecond = System.Math.Round(totalOutputInKB / Time.realtimeSinceStartup, 3);
            mbPerSecond = System.Math.Round(totalOutputInMB / Time.realtimeSinceStartup, 3);

            timeSinceLastSecondUpdate += Time.deltaTime;
            
            if (logStatistics)
            {
                if (timeSinceLastSecondUpdate > 1)
                {
                    timeSinceLastSecondUpdate -= 1;

                    lastSecondOutputInBytes = ServerNetworkStatistics.outputBytesTotal - previousTotalBytes;
                    previousTotalBytes = ServerNetworkStatistics.outputBytesTotal;
                
                    Debug.Log($"Last Second Output (Bytes): {lastSecondOutputInBytes}");
                }
                
                if (Time.realtimeSinceStartup - timeSincelastLog > consoleLogFrequencyInSeconds)
                {
                    // log stuff...
                    
                    Debug.Log($"Total Output (KB): {totalOutputInKB}");
                    Debug.Log($"Connected Players: {ServerNetworkStatistics.currentConnections}");

                    timeSincelastLog = Time.realtimeSinceStartup;
                }
            }

            // if (Input.GetKeyUp(KeyCode.R))
            // {
            //     var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //     entityManager.CreateEntity(ComponentType.ReadOnly<RestartServerCommand>());
            //     // RestartServerCommand
            //     // SceneManager.LoadScene("ReloadScene");
            // }
        }
        
    }
}