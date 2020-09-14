using System.Collections;
using NaiveNetworkGame.Client.Systems;
using Unity.Entities;
using UnityEngine;

namespace Client
{
    public class ClientBehaviour : MonoBehaviour
    {
        private void Start ()
        {
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(1);
            
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var clientEntity = entityManager.CreateEntity();
            // entityManager.AddSharedComponentData(clientEntity, new ClientSingleton());
            entityManager.AddComponentData(clientEntity, new StartClientCommand());
        }
    }
}