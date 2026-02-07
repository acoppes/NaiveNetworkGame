using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public struct AttackComponent : IComponentData
    {
        public float damage;
        
        public float range;
        public float chaseRange;

        public float attackTime;
        public float duration;

        public float reload;

        public float reloadRandom;
        // public float chaseRange;

        public float3 chaseCenter;
    }
    
    public struct DisableAttackComponent : IComponentData
    {
        
    }
    
    public struct AttackTargetComponent : IComponentData
    {
        public Entity target;
    }
    
    public struct ChaseTargetComponent : IComponentData
    {
        public Entity target;
    }
    
    public class AttackAuthoring : MonoBehaviour
    {
        public float damage;
        
        public float range;
        public float chaseRange;

        public float attackTime;
        public float duration;

        public float reload;

        public float reloadRandom;
        // public float chaseRange;

        public float3 chaseCenter;

        private class AttackBaker : Baker<AttackAuthoring>
        {
            public override void Bake(AttackAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AttackComponent()
                {
                    damage = authoring.damage,
                
                    range = authoring.range,
                    chaseRange = authoring.chaseRange,

                    attackTime = authoring.attackTime,
                    duration = authoring.duration,

                    reload = authoring.reload,

                    reloadRandom = authoring.reloadRandom,
                    // chaseRange = chaseRange,

                    chaseCenter = authoring.chaseCenter,
                });
            }
        }
    }
}