using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct ActivePlayerComponent : IComponentData
    {
    
    }
    
    public class ActivePlayerAuthoring : MonoBehaviour
    {
        private class ActivePlayerBaker : Baker<ActivePlayerAuthoring>
        {
            public override void Bake(ActivePlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ActivePlayerComponent());
            }
        }
    }
}