using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct UnitSpritesAnimationsComponent : IComponentData
    {
        public FixedList32Bytes<UnityObjectRef<AnimationDefinition>> animations;
        
        // public UnityObjectRef<AnimationDefinition> idle;
        // public UnityObjectRef<AnimationDefinition> walk;
    }

    public struct UnitSpritePlayAnimationComponent : IComponentData
    {
        public int animation;
        // public FixedString32Bytes name;
    }
    
    public class UnitSpritesAnimationsAuthoring : MonoBehaviour
    {
        public List<AnimationDefinition> animations;

        public int startingAnimation;
        
        // public string startingAnimation;
        
        // public AnimationDefinition idle;
        // public AnimationDefinition walk;

        public class UnitSpritesAnimationsComponentBaker : Baker<UnitSpritesAnimationsAuthoring>
        {
            public override void Bake(UnitSpritesAnimationsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                if (authoring.animations == null)
                {
                    return;
                }

                var animationsArray = new FixedList32Bytes<UnityObjectRef<AnimationDefinition>>();

                for (var i = 0; i < authoring.animations.Count; i++)
                {
                    var animation = authoring.animations[i];
                    animationsArray.Add(animation);
                    // animationsArray[i] = animation;
                }

                AddComponent(entity, new UnitSpritesAnimationsComponent
                {
                    // idle = authoring.idle,
                    // walk = authoring.walk,
                    animations = animationsArray
                });
                AddComponent(entity, new UnitSpritePlayAnimationComponent()
                {
                    animation = authoring.startingAnimation,
                    // name = authoring.startingAnimation
                });
            }
        }
    }
}