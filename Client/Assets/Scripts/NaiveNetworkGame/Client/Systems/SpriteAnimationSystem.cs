using NaiveNetworkGame.Client.Components;
using Unity.Burst;
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

                var animationDefinition = (AnimationDefinition) newAnimation.Value;

                animation.ValueRW.currentAnimation = play.ValueRO.animation;
                animation.ValueRW.currentFrame = 0;
                animation.ValueRW.frameTime = 1f / animationDefinition.fps;
                animation.ValueRW.totalFrames = animationDefinition.sprites.Count;
                
                ecb.RemoveComponent(e, typeof(UnitSpritePlayAnimationComponent));
            }
            
            // TODO: TEST RUNNING THIS IN PARALLEL WITH JOBS
            // foreach (var animationComponent in 
            //          SystemAPI.Query<RefRW<SpriteAnimationComponent>>())
            // {
            //     ref var animator = ref animationComponent.ValueRW;
            //     
            //     animator.currentTime += SystemAPI.Time.DeltaTime;
            //
            //     if (animator.currentTime > animator.frameTime)
            //     {
            //         animator.currentTime -= animator.frameTime;
            //         animator.currentFrame++;
            //     }
            //
            //     if (animator.currentFrame >= animator.totalFrames)
            //     {
            //         animator.currentFrame = 0;
            //     }
            // }
            
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
    
    [BurstCompile]
    [UpdateAfter(typeof(SpriteAnimationSystem))]
    public partial struct SpriteAnimationParallelSystem : ISystem
    {
        [BurstCompile]
        public partial struct AnimatorJob : IJobEntity
        {
            public float dt;

            // IJobEntity generates a query for entities that have LocalTransform and
            // RotationSpeed.
            private void Execute(ref SpriteAnimationComponent animator)
            {
                animator.currentTime += dt;

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
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AnimatorJob()
            {
                dt = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();
        }
    }
}