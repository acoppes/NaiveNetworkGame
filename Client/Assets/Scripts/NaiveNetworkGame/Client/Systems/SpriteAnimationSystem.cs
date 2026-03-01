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
            
            // foreach (var (animationComponent, unitSpritesAnimationsComponent) in 
            //          SystemAPI.Query<RefRW<SpriteAnimationComponent>, RefRO<UnitSpritesAnimationsComponent>>())
            // {
            //     if (animationComponent.ValueRW.animation == null)
            //     {
            //         animationComponent.ValueRW.animation = unitSpritesAnimationsComponent.ValueRO.idle;
            //     }
            // }
            
            foreach (var (animation, unitSprites, play, e) in 
                     SystemAPI.Query<RefRW<SpriteAnimationComponent>, RefRO<UnitSpritesAnimationsComponent>, RefRO<UnitSpritePlayAnimationComponent>>()
                         .WithEntityAccess())
            {
                var newAnimation = unitSprites.ValueRO.animations[play.ValueRO.animation];
                animation.ValueRW.animation = newAnimation;
                ecb.RemoveComponent(e, typeof(UnitSpritePlayAnimationComponent));
            }
            
            foreach (var animationComponent in 
                     SystemAPI.Query<RefRW<SpriteAnimationComponent>>())
            {
                if (animationComponent.ValueRO.animation == null)
                {
                    continue;
                }
                
                var animation = animationComponent.ValueRO.animation.Value;
                
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
                spriteRenderer.Value.sprite = spriteAnimation.ValueRO.animation.Value.sprites[spriteAnimation.ValueRO.current];
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}