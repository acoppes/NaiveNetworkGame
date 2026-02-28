using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public class SpriteAnimationComponent : IComponentData
    {
        public List<Sprite> sprites;
        public float fps;
        public float frameTime => 1f / fps;
        public float currentTime;
        public int current;
    }

    public class SpriteAnimationComponentAuthoring : MonoBehaviour
    {
        public List<Sprite> sprites;
        public float fps;

        public class SpriteAnimationComponentBaker : Baker<SpriteAnimationComponentAuthoring>
        {
            public override void Bake(SpriteAnimationComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new SpriteAnimationComponent
                {
                    sprites = authoring.sprites,
                    fps = authoring.fps
                });
            }
        }
    }
}