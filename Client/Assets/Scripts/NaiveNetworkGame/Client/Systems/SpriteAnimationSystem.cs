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
                     SystemAPI.Query<SpriteAnimationComponent>())
            {
                animationComponent.currentTime += SystemAPI.Time.DeltaTime;

                if (animationComponent.currentTime > animationComponent.frameTime)
                {
                    animationComponent.currentTime -= animationComponent.frameTime;
                    animationComponent.current++;
                }

                if (animationComponent.current >= animationComponent.sprites.Count)
                {
                    animationComponent.current = 0;
                }
            }
            
            foreach (var (animationComponent, spriteRenderer) in 
                     SystemAPI.Query<SpriteAnimationComponent, SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>>())
            {
                spriteRenderer.Value.sprite = animationComponent.sprites[animationComponent.current];
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}