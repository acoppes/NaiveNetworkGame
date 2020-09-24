using System.Collections;
using Client;
using NaiveNetworkGame.Client;
using Unity.Collections;
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
            
            yield return new WaitForSeconds(1.0f);

            var playerInputStateQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadWrite<PlayerInputState>());

            var entities = playerInputStateQuery.ToEntityArray(Allocator.TempJob);
            var inputStates = playerInputStateQuery.ToComponentDataArray<PlayerInputState>(Allocator.TempJob);

            for (int i = 0; i < inputStates.Length; i++)
            {
                var inputState = inputStates[i];
                inputState.spawnActionPressed = true;
                
                entityManager.SetComponentData(entities[i], inputState);
            }

            entities.Dispose();
            inputStates.Dispose();
        }

    }
}
