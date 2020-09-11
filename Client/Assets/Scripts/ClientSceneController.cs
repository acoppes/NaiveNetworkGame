using System;
using Client;
using Mockups;
using NaiveNetworkGame.Common;
using Unity.Entities;
using UnityEngine;

namespace Scenes
{
    public struct UserInterfaceComponent : ISharedComponentData, IEquatable<UserInterfaceComponent>
    {
        public FixedNumbersLabel goldLabel;

        public bool Equals(UserInterfaceComponent other)
        {
            return Equals(goldLabel, other.goldLabel);
        }

        public override bool Equals(object obj)
        {
            return obj is UserInterfaceComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (goldLabel != null ? goldLabel.GetHashCode() : 0);
        }
    }
    
    public struct ClientPrefabsSharedComponent : ISharedComponentData, IEquatable<ClientPrefabsSharedComponent>
    {
        public GameObject confirmActionPrefab;

        public bool Equals(ClientPrefabsSharedComponent other)
        {
            return Equals(confirmActionPrefab, other.confirmActionPrefab);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientPrefabsSharedComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (confirmActionPrefab != null ? confirmActionPrefab.GetHashCode() : 0);
        }
    }
    
    public class ClientSceneController : MonoBehaviour
    {
        public Camera camera;

        // public GameObject prefab;
        public Transform parent;
        
        public GameObject actionPrefab;
        
        public FixedNumbersLabel goldLabel;

        // public PlayerButton playerButton;

        // public Button spawnUnitButton;

        private EntityQuery gameStateQuery;

        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            gameStateQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadWrite<PlayerInputState>());
            
            var playerEntity = entityManager.CreateEntity();

            entityManager.AddComponentData(playerEntity, new PlayerInputState());
            entityManager.AddComponentData(playerEntity, new PlayerController());
            entityManager.AddSharedComponentData(playerEntity, new UserInterfaceComponent
            {
                goldLabel = goldLabel
            });
            
            // var playerInputState = entityManager.GetComponentData<PlayerInputState>(gameState);
            // gameStateQuery.SetSingleton(playerInputState);
            
            ModelProviderSingleton.Instance.SetRoot(parent);

            // var query = entityManager.CreateEntityQuery(
            //     ComponentType.ReadWrite<ClientPrefabsSharedComponent>());
            var clientPrefabsEntity = entityManager.CreateEntity();
            entityManager.AddSharedComponentData(clientPrefabsEntity, new ClientPrefabsSharedComponent
            {
                confirmActionPrefab = actionPrefab
            });
            
        }

        public void ToggleSpawning()
        {
            var playerInputState = gameStateQuery.GetSingleton<PlayerInputState>();
            playerInputState.spawnActionPressed = !playerInputState.spawnActionPressed;
            gameStateQuery.SetSingleton(playerInputState);
        }

        public bool IsWaitingForSpawing()
        {
            var playerInputState = gameStateQuery.GetSingleton<PlayerInputState>();
            return playerInputState.spawnActionPressed;
        }
    }
}
