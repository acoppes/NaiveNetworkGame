using System;
using System.Collections;
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


        // private void Update()
        // {
        //     // update spawn button state...
        //     
        //     if (Input.GetMouseButtonUp(1))
        //     {
        //         var mousePosition = Input.mousePosition;
        //         var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //
        //         worldPosition.z = 0;
        //
        //         var actionInstance = Instantiate(actionPrefab, worldPosition, Quaternion.identity);
        //         StartCoroutine(DestroyActionOnComplete(actionInstance));
        //     }
        // }

        // private IEnumerator DestroyActionOnComplete(GameObject actionInstance)
        // {
        //     var animator = actionInstance.GetComponent<Animator>();
        //     var hiddenState = Animator.StringToHash("Hidden");
        //     
        //     animator.SetTrigger("Action");
        //
        //     yield return null;
        //     
        //     yield return new WaitUntil(delegate
        //     {
        //         var currentState = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        //         return currentState == hiddenState;
        //     });
        //     
        //     Destroy(actionInstance);
        // }

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
