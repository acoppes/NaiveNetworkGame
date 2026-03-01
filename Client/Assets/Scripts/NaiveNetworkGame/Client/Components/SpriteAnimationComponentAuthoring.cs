using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct SpriteAnimationComponent : IComponentData
    {
        public UnityObjectRef<AnimationDefinition> animationReference;
        public float currentTime;
        public int current;
    }

    public class SpriteAnimationComponentAuthoring : MonoBehaviour
    {
        public AnimationDefinition animationDefinition;

        public class SpriteAnimationComponentBaker : Baker<SpriteAnimationComponentAuthoring>
        {
            public override void Bake(SpriteAnimationComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SpriteAnimationComponent
                {
                    animationReference = authoring.animationDefinition,
                });
            }
        }
    }
}