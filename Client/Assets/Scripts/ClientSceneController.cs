using System;
using System.Collections;
using Client;
using NaiveNetworkGame.Client;
using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Client.Systems;
using NaiveNetworkGame.Common;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public struct UserInterfaceComponent : ISharedComponentData, IEquatable<UserInterfaceComponent>
    {
        public PlayerButton spawnUnitButton;
        public FixedNumbersLabel goldLabel;
        public PlayerStatsUI playerStats;

        public bool Equals(UserInterfaceComponent other)
        {
            return Equals(spawnUnitButton, other.spawnUnitButton) && Equals(goldLabel, other.goldLabel) && Equals(playerStats, other.playerStats);
        }

        public override bool Equals(object obj)
        {
            return obj is UserInterfaceComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (spawnUnitButton != null ? spawnUnitButton.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (goldLabel != null ? goldLabel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (playerStats != null ? playerStats.GetHashCode() : 0);
                return hashCode;
            }
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

        public PlayerButton spawnUnitButton;
        public FixedNumbersLabel goldLabel;
        public PlayerStatsUI playerStats;
        
        // public PlayerButton playerButton;

        // public Button spawnUnitButton;

        private EntityQuery gameStateQuery;
        private EntityQuery playerControllerQuery;

        public CanvasGroup uiGroup;

        public Button disconnectButton;

        public Text connectionStateText;

        public GameObject receivedBytesObject;
        public Text receivedBytesText;
        
        public GameObject connectedTimeObject;
        public Text connectedTimeText;

        public bool autoCreatePlayer = true;

        private void Start()
        {
            ConnectionState.connectedTime = 0;
            ConnectionState.totalReceivedBytes = 0;
            ConnectionState.currentState = ConnectionState.State.Connecting;
            
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            gameStateQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadWrite<PlayerInputState>());
            
            playerControllerQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadWrite<PlayerController>());

            if (autoCreatePlayer)
            {
                var playerEntity = entityManager.CreateEntity();

                entityManager.AddComponentData(playerEntity, new PlayerInputState());
                entityManager.AddComponentData(playerEntity, new PlayerController());
                entityManager.AddComponentData(playerEntity, new LocalPlayer());
                entityManager.AddSharedComponentData(playerEntity, new UserInterfaceComponent
                {
                    goldLabel = goldLabel,
                    playerStats = playerStats,
                    spawnUnitButton = spawnUnitButton
                });
            }
            
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
            
            disconnectButton.onClick.AddListener(OnDisconnectButtonPressed);
        }

        private void OnDisconnectButtonPressed()
        {
            StartCoroutine(DisconnectAndCloseApplication());
        }

        private IEnumerator DisconnectAndCloseApplication()
        {
            disconnectButton.enabled = false;
            
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var clientEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(clientEntity, new DisconnectClientCommand());

            yield return new WaitForSeconds(1.0f);
            
            Application.Quit(0);
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
            #endif
        }

        private ConnectionState.State previousState = ConnectionState.State.Disconnected;

        private void LateUpdate()
        {
            if (ConnectionState.currentState == ConnectionState.State.Connected)
                ConnectionState.connectedTime += Time.deltaTime;

            if (uiGroup != null)
            {
                uiGroup.interactable = ConnectionState.currentState == ConnectionState.State.Connected;
                uiGroup.alpha = uiGroup.interactable ? 1.0f : 0.0f;
            }
            
            if (connectionStateText != null)
            {
                if (previousState != ConnectionState.currentState)
                {
                    previousState = ConnectionState.currentState;

                    switch (ConnectionState.currentState)
                    {
                        case ConnectionState.State.Connected:
                            // connectionStateText.text = "Connected";
                            connectionStateText.gameObject.SetActive(false);
                            receivedBytesObject.SetActive(true);
                            connectedTimeObject.SetActive(true);
                            break;
                        case ConnectionState.State.Connecting:
                            connectionStateText.gameObject.SetActive(true);
                            connectionStateText.text = "Connecting to server...";
                            receivedBytesObject.SetActive(false);
                            connectedTimeObject.SetActive(false);
                            break;
                        case ConnectionState.State.Disconnected:
                            connectionStateText.gameObject.SetActive(true);
                            connectionStateText.text = "Disconnected from server...";
                            // receivedBytesObject.SetActive(false);
                            break;
                    }
                }
            }

            receivedBytesText.text = $"{ConnectionState.totalReceivedBytes / 1024} KB";
            connectedTimeText.text = $"{ConnectionState.connectedTime:0.}s";
        }

        public void ToggleSpawning()
        {
            var playerInputState = gameStateQuery.GetSingleton<PlayerInputState>();
            playerInputState.spawnActionPressed = !playerInputState.spawnActionPressed;
            gameStateQuery.SetSingleton(playerInputState);
        }

        public bool IsSpawnEnabled()
        {
            var playerController = playerControllerQuery.GetSingleton<PlayerController>();
            return playerController.currentUnits < playerController.maxUnits;
        }
    }
}
