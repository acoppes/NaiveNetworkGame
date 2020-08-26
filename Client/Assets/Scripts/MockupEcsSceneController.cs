using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Mockups
{
    public class MockupEcsSceneController : MonoBehaviour
    {
        // Update is called once per frame
        private void Update()
        {
            if (Input.GetMouseButtonUp(1))
            {
                var modelProvider = ModelProviderSingleton.Instance;
                
                var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                position.z = 0;
                
                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var entity = manager.CreateEntity();
                manager.AddSharedComponentData(entity, new ModelPrefabComponent
                {
                    prefab = modelProvider.prefabs[UnityEngine.Random.Range(0, modelProvider.prefabs.Length)]
                });
                manager.AddComponentData(entity, new Translation
                {
                    Value = position
                });

            }
        }
    }
}
