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
    
    public class AttackAuthoring : MonoBehaviour, IConvertGameObjectToEntity
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
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AttackComponent()
            {
                damage = damage,
                
                range = range,
                chaseRange = chaseRange,

                attackTime = attackTime,
                duration = duration,

                reload = reload,

                reloadRandom = reloadRandom,
                // chaseRange = chaseRange,

                chaseCenter = chaseCenter,
            });
        }
    }
}