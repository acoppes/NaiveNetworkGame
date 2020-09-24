using System.Collections;
using Client;
using NaiveNetworkGame.Client;
using Unity.Entities;
using UnityEngine;

namespace Scenes.Tests
{
    public class TestUnitsAttackController : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            StartCoroutine(StartTest());
        }

        private IEnumerator StartTest()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            yield return new WaitUntil(() => ConnectionState.currentState == ConnectionState.State.Connected);

            // var query = entityManager.CreateEntityQuery(ComponentType.ReadWrite<ClientSingleton>());
            // var clientSingleton = query.GetSingletonEntity<ClientSingleton>();
            
            var playerInputStateQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadWrite<PlayerInputState>());
            
            var playerInputState = playerInputStateQuery.GetSingleton<PlayerInputState>();
            playerInputState.spawnActionPressed = !playerInputState.spawnActionPressed;
            playerInputStateQuery.SetSingleton(playerInputState);
        }

    }
}
