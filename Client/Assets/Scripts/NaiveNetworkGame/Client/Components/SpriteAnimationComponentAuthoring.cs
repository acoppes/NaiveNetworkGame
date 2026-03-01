using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct SpriteAnimationComponent : IComponentData
    {
        public float currentTime;
        public int currentFrame;
        
        public int currentAnimation;
        
        public int totalFrames;
        public float frameTime;
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