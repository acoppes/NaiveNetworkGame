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

                animation.ValueRW.currentAnimation = play.ValueRO.animation;
                animation.ValueRW.currentFrame = 0;
                animation.ValueRW.frameTime = newAnimation.Value.frameTime;
                animation.ValueRW.totalFrames = newAnimation.Value.sprites.Count;
                
                ecb.RemoveComponent(e, typeof(UnitSpritePlayAnimationComponent));
            }
            
            // TODO: TEST RUNNING THIS IN PARALLEL WITH JOBS
            foreach (var animationComponent in 
                     SystemAPI.Query<RefRW<SpriteAnimationComponent>>())
            {
                ref var animator = ref animationComponent.ValueRW;
                
                animator.currentTime += SystemAPI.Time.DeltaTime;

                if (animator.currentTime > animator.frameTime)
                {
                    animator.currentTime -= animator.frameTime;
                    animator.currentFrame++;
                }

                if (animator.currentFrame >= animator.totalFrames)
                {
                    animator.currentFrame = 0;
                }
            }
            
            foreach (var (animator, animations, renderer) in 
                     SystemAPI.Query<RefRO<SpriteAnimationComponent>, RefRO<UnitSpritesAnimationsComponent>, SystemAPI.ManagedAPI.UnityEngineComponent<SpriteRenderer>>())
            {
                var animationRef = animations.ValueRO.animations[animator.ValueRO.currentAnimation];
                renderer.Value.sprite = animationRef.Value.sprites[animator.ValueRO.currentFrame];
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}