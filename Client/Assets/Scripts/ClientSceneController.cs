using System;
using Client;
using Mockups;
using Unity.Entities;
using UnityEngine;

namespace Scenes
{
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

        // public PlayerButton playerButton;

        // public Button spawnUnitButton;

        private EntityQuery gameStateQuery;

        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            gameStateQuery = entityManager.CreateEntityQuery(ComponentType.ReadWrite<PlayerInputState>());
            var gameState = entityManager.CreateEntity(ComponentType.ReadWrite<PlayerInputState>());

            var clientGameState = entityManager.GetComponentData<PlayerInputState>(gameState);
            gameStateQuery.SetSingleton(clientGameState);
            
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
            var clientGameState = gameStateQuery.GetSingleton<PlayerInputState>();
            clientGameState.spawnActionPressed = !clientGameState.spawnActionPressed;
            gameStateQuery.SetSingleton(clientGameState);
        }

        public bool IsWaitingForSpawing()
        {
            var clientGameState = gameStateQuery.GetSingleton<PlayerInputState>();
            return clientGameState.spawnActionPressed;
        }
    }
}
