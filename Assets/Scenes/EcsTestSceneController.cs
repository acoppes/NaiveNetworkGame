using Server;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes
{
    public class EcsTestSceneController : MonoBehaviour
    {
        public Camera camera;

        private void Update()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (Input.GetMouseButtonUp(0))
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = camera.ScreenToWorldPoint(mousePosition);

                var entity = entityManager.CreateEntity();
                
                entityManager.AddComponentData(entity, new PendingPlayerAction
                {
                    player = 0,
                    command = 0,
                    target = new float2(worldPosition.x, worldPosition.y)
                });
            }
        }
    }
}
