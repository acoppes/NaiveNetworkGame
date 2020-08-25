﻿using Unity.Entities;
using Unity.Entities.UniversalDelegates;
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
        }
        
    }
}