using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct CreateLocalPlayerCommand : IComponentData
    {
        public bool active;
    }
    
    public struct LocalClientController : IComponentData
    {
        public Entity localPlayerPrefab;
    }
    
    public class LocalClientControllerAuthoring : MonoBehaviour
    {
        public GameObject localPlayerPrefab;

        public class LocalClientControllerAuthoringBaker : Baker<LocalClientControllerAuthoring>
        {
            public override void Bake(LocalClientControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new LocalClientController
                    {
                        localPlayerPrefab = GetEntity(authoring.localPlayerPrefab, TransformUsageFlags.Dynamic)
                    });
            }
        }
    }
}