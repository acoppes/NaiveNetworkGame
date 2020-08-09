using Client;
using Server;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes
{
    public class EcsTestClientSceneController : MonoBehaviour
    {
        public Camera camera;

        // public uint player;
        public int button;

        public ClientBehaviour clientBehaviour;

        private void Update()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (clientBehaviour.clientNetworkManager.networkPlayerId == -1)
                return;

            if (Input.GetMouseButtonUp(button))
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = camera.ScreenToWorldPoint(mousePosition);

                var entity = entityManager.CreateEntity(ComponentType.ReadOnly<ClientOnly>());
                entityManager.AddComponentData(entity, new PendingPlayerAction
                {
                    player = (uint) clientBehaviour.clientNetworkManager.networkPlayerId,
                    command = 0,
                    target = new float2(worldPosition.x, worldPosition.y)
                });
            }
        }
    }
}
