using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct SpriteAnimationComponent : IComponentData
    {
        public UnityObjectRef<AnimationDefinition> animation;
        public float currentTime;
        public int current;
    }

    public class SpriteAnimationComponentAuthoring : MonoBehaviour
    {
        public class SpriteAnimationComponentBaker : Baker<SpriteAnimationComponentAuthoring>
        {
            public override void Bake(SpriteAnimationComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SpriteAnimationComponent());
            }
        }
    }
}