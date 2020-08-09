using Server;
using Unity.Entities;
using UnityEngine;

namespace Client
{
    public class ClientBehaviour : MonoBehaviour
    {
        private void Start ()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var clientEntity = entityManager.CreateEntity(ComponentType.ReadOnly<ClientOnly>());
            entityManager.AddSharedComponentData(clientEntity, new NetworkManagerSharedComponent());
            entityManager.AddComponentData(clientEntity, new ClientStartComponent());
        }
    }
}