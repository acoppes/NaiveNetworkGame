using NaiveNetworkGame.Client.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    public partial struct SpriteAnimationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var animationComponent in 
                     SystemAPI.Query<RefRW<SpriteAnimationComponent>>())
            {
                var animation = animationComponent.ValueRO.animationReference.Value;
                
                animationComponent.ValueRW.currentTime += SystemAPI.Time.DeltaTime;

                if (animationComponent.ValueRW.currentTime > animation.frameTime)
                {
                    animationComponent.ValueRW.currentTime -= animation.frameTime;
                    animationComponent.ValueRW.current++;
                }

                if (animationComponent.ValueRW.current >= animation.sprites.Count)
                {
                    animationComponent.ValueRW.current = 0;
                }
            }
            
            foreach (var (spriteAnimation, spriteRenderer) in 
                     SystemAPI.Query<RefRO<SpriteAnimationComponent>, SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>>())
            {
                spriteRenderer.Value.sprite = spriteAnimation.ValueRO.animationReference.Value.sprites[spriteAnimation.ValueRO.current];
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}