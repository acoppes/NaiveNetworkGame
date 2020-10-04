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
        public int unitsCount = 1;
        
        // Start is called before the first frame update
        private void Start()
        {
            StartCoroutine(StartTest());
        }

        private IEnumerator StartTest()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            yield return new WaitUntil(() => ConnectionState.currentState == ConnectionState.State.Connected);
            
            yield return new WaitForSeconds(0.1f);

            var playerInputStateQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadWrite<PlayerPendingAction>());

            while (unitsCount > 0)
            {
                var entities = playerInputStateQuery.ToEntityArray(Allocator.TempJob);
                var inputStates = playerInputStateQuery.ToComponentDataArray<PlayerPendingAction>(Allocator.TempJob);

                for (int i = 0; i < inputStates.Length; i++)
                {
                    var inputState = inputStates[i];
                    
                    inputState.pending = true;
                    inputState.actionType = 2;
                    inputState.unitType = 0;

                    entityManager.SetComponentData(entities[i], inputState);
                }

                entities.Dispose();
                inputStates.Dispose();

                unitsCount--;
                
                yield return null;
            }
        }

    }
}
