using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Server
{
    public struct RestartServerCommand : IComponentData
    {
        
    }
    
    public class RestartServerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
     
            Entities
            .WithAll<RestartServerCommand>()
            .ForEach(delegate(Entity e)
            {
                PostUpdateCommands.DestroyEntity(e);
                // PostUpdateCommands.DestroyEntity(EntityManager.UniversalQuery);
                SceneManager.LoadScene("ReloadScene");
            });
        }
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

        public float sendTranslationStateFrequency = 0.1f;
        public float sendGameStateFrequency = 0.1f;
        
        public GameObject unitPrefab;
        public GameObject treePrefab;
        
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

        public bool staticObjectsEnabled;

        private void Awake()
        {
            ServerNetworkStaticData.staticObjectsEnabled = staticObjectsEnabled;
        }

        private void Start ()
        {
            ServerNetworkStaticData.sendGameStateFrequency = sendGameStateFrequency;
            ServerNetworkStaticData.sendTranslationStateFrequency = sendTranslationStateFrequency;
            
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

            var startServerCommand = entityManager.CreateEntity();
            entityManager.AddComponentData(startServerCommand, new StartServerCommand
            {
                port = port
            });
            
            // create multiple player controllers, all disabled....
            // with each connection, remove disabled component.

            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            
            var prefabsEntity = entityManager.CreateEntity();
            entityManager.AddSharedComponentData(prefabsEntity, new PrefabsSharedComponent
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
            Application.targetFrameRate = targetFrameRate;
            
            totalOutputInBytes = ServerNetworkStatistics.outputBytesTotal;
            lastFrameOutputInBytes = ServerNetworkStatistics.outputBytesLastFrame;

            totalOutputInKB = System.Math.Round(ServerNetworkStatistics.outputBytesTotal / 1024.0f, 3);
            totalOutputInMB = System.Math.Round(totalOutputInKB / 1024.0f, 3);

            bytesPerSecond = System.Math.Round(ServerNetworkStatistics.outputBytesTotal / Time.realtimeSinceStartup, 3);
            kbPerSecond = System.Math.Round(totalOutputInKB / Time.realtimeSinceStartup, 3);
            mbPerSecond = System.Math.Round(totalOutputInMB / Time.realtimeSinceStartup, 3);

            timeSinceLastSecondUpdate += Time.deltaTime;
            
            if (timeSinceLastSecondUpdate > 1)
            {
                timeSinceLastSecondUpdate -= 1;

                lastSecondOutputInBytes = ServerNetworkStatistics.outputBytesTotal - previousTotalBytes;
                previousTotalBytes = ServerNetworkStatistics.outputBytesTotal;
                
                Debug.Log($"Last Second Output (Bytes): {lastSecondOutputInBytes}");
            }
            
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