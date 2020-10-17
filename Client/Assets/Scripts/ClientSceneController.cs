using System;
using System.Collections;
using Client;
using NaiveNetworkGame.Client;
using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Client.Systems;
using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

        public UserInterface userInterface;

        private EntityQuery playerControllerQuery;

        public Button disconnectButton;
        [FormerlySerializedAs("forceStartButton")] public Button secondPlayerButton;
        
        public Text connectionStateText;

        public GameObject receivedBytesObject;
        public Text receivedBytesText;
        
        public GameObject connectedTimeObject;
        public Text connectedTimeText;

        public Text latencyText;

        private EntityManager entityManager;

        public GameObject secondLocalPlayer;

        private bool secondPlayerAdded = false;
        
        private void Start()
        {
            ConnectionState.connectedTime = 0;
            ConnectionState.totalReceivedBytes = 0;
            ConnectionState.currentState = ConnectionState.State.Connecting;
            
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            playerControllerQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadWrite<LocalPlayerController>(), 
                ComponentType.ReadOnly<ActivePlayer>(), 
                ComponentType.ReadWrite<PlayerPendingAction>());
            
            var userInterfaceEntity = entityManager.CreateEntity();
            entityManager.AddSharedComponentData(userInterfaceEntity, new UserInterfaceSharedComponent
            {
                userInterface = userInterface
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
            
            disconnectButton.onClick.AddListener(OnDisconnectButtonPressed);
            
            secondPlayerButton.onClick.AddListener(delegate
            {
                
                // create server simulation
                if (!secondPlayerAdded)
                {
                    secondLocalPlayer.SetActive(true);
                    secondPlayerAdded = true;

                    secondPlayerButton.GetComponentInChildren<Text>().text = "Switch Player";
                }
                else
                {
                    // switch player
                    entityManager.CreateEntity(typeof(SwitchLocalPlayerAction));
                }
                
            });
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
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                entityManager.CreateEntity(typeof(SwitchLocalPlayerAction));
            }

            if (ConnectionState.currentState == ConnectionState.State.SimulationRunning)
                ConnectionState.connectedTime += Time.deltaTime;

            if (userInterface != null)
            {
                userInterface.visible = ConnectionState.currentState == ConnectionState.State.SimulationRunning;
            }
            
            if (connectionStateText != null)
            {
                if (previousState != ConnectionState.currentState)
                {
                    previousState = ConnectionState.currentState;

                    switch (ConnectionState.currentState)
                    {
                        case ConnectionState.State.WaitingForPlayers:
                            connectionStateText.gameObject.SetActive(true);
                            connectionStateText.text = "Waiting for other players...";
                            receivedBytesObject.SetActive(false);
                            connectedTimeObject.SetActive(false);
                            break;
                        case ConnectionState.State.SimulationRunning:
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
            
            if (latencyText != null)
                latencyText.text = $"{Mathf.RoundToInt((float) (ConnectionState.latency * 1000.0f))}ms";

            // var serverQuery = entityManager.CreateEntityQuery(typeof(ServerSingleton));
            // var hasServer = serverQuery.CalculateEntityCount() > 0;
            
            var serverSimulationStarted = entityManager.CreateEntityQuery(
                typeof(ServerSimulation)).CalculateEntityCount() > 0;

            // if (hasServer)
            // {
            //     // var s = serverQuery.GetSingletonEntity();
            //     var s = entityManager.GetSharedComponentData<ServerSingleton>(serverQuery.GetSingletonEntity());
            //     hasServer = s.networkManager != null;
            // }
            
            if (!secondPlayerAdded)
                secondPlayerButton.gameObject.SetActive(!serverSimulationStarted);
        }

        public void OnPlayerAction(PlayerActionAsset playerAction)
        {
            // another option is to iterate in every button of active player....
            if (playerControllerQuery.TryGetSingletonEntity(out var playerEntity))
            {
                // var playerEntity = playerControllerQuery.GetSingletonEntity();
                var playerInput = entityManager.GetComponentData<PlayerPendingAction>(playerEntity);

                playerInput.pending = true;
                playerInput.actionType = playerAction.type;
                playerInput.unitType = playerAction.unitType;
                playerControllerQuery.SetSingleton(playerInput);

                var playerActions = entityManager.GetBuffer<PlayerAction>(playerEntity);
                var playerController = entityManager.GetComponentData<LocalPlayerController>(playerEntity);

                for (var i = 0; i < playerActions.Length; i++)
                {
                    var pa = playerActions[i];
                    if (pa.type == playerAction.unitType)
                    {
                        playerController.gold -= pa.cost;
                        entityManager.SetComponentData(playerEntity, playerController);
                    }
                }


                // entityManager.AddComponent(playerEntity, new PlayerPendingActions());
            }
        }
    }
}
