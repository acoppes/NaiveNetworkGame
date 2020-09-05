using System;
using System.Collections;
using Client;
using Mockups;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    // public struct UnitGameState : IBufferElementData
    // {
    //     public int frame;
    //     public float time;
    //     public float2 translation;
    // }

    // public struct ClientModelRootSharedComponent : ISharedComponentData, IEquatable<ClientModelRootSharedComponent>
    // {
    //     public Transform parent;
    //
    //     public bool Equals(ClientModelRootSharedComponent other)
    //     {
    //         return Equals(parent, other.parent);
    //     }
    //
    //     public override bool Equals(object obj)
    //     {
    //         return obj is ClientModelRootSharedComponent other && Equals(other);
    //     }
    //
    //     public override int GetHashCode()
    //     {
    //         return (parent != null ? parent.GetHashCode() : 0);
    //     }
    // }

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
            
            // var clientModelRoot = entityManager.CreateEntity();
            // entityManager.AddSharedComponentData(clientModelRoot, new ClientModelRootSharedComponent
            // {
            //     parent = parent
            // });

            ModelProviderSingleton.Instance.SetRoot(parent);
            
            // spawnUnitButton.onClick.AddListener(OnSpawnButtonPressed);
        }

        // private void OnSpawnButtonPressed()
        // {
        //     // change game state to spawning unit...
        //     
        //     // with input, send player action spawn
        //     
        //     // if pressed again, cancel state
        //
        //     var clientGameState = gameStateQuery.GetSingleton<PlayerInputState>();
        //
        //     if (clientGameState.spawningUnit)
        //     {
        //         clientGameState.spawningUnit = false;
        //         // revert visual stuff or update that in update
        //     }
        //     else
        //     {
        //         clientGameState.spawningUnit = true;
        //         // set visual stuff??
        //     }
        //     
        //     gameStateQuery.SetSingleton(clientGameState);
        // }

        private void LateUpdate()
        {
            var clientGameState = gameStateQuery.GetSingleton<PlayerInputState>();
            
            
        }

        private void Update()
        {
            // update spawn button state...
            
            if (Input.GetMouseButtonUp(1))
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

                worldPosition.z = 0;

                var actionInstance = Instantiate(actionPrefab, worldPosition, Quaternion.identity);
                StartCoroutine(DestroyActionOnComplete(actionInstance));
            }
        }

        private IEnumerator DestroyActionOnComplete(GameObject actionInstance)
        {
            var animator = actionInstance.GetComponent<Animator>();
            var hiddenState = Animator.StringToHash("Hidden");
            
            animator.SetTrigger("Action");

            yield return null;
            
            yield return new WaitUntil(delegate
            {
                var currentState = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
                return currentState == hiddenState;
            });
            
            Destroy(actionInstance);
        }

        public void ToggleSpawning()
        {
            var clientGameState = gameStateQuery.GetSingleton<PlayerInputState>();
            clientGameState.spawnWaitingForPosition = !clientGameState.spawnWaitingForPosition;
            gameStateQuery.SetSingleton(clientGameState);
        }

        public bool IsWaitingForSpawing()
        {
            var clientGameState = gameStateQuery.GetSingleton<PlayerInputState>();
            return clientGameState.spawnWaitingForPosition;
        }
    }
}
