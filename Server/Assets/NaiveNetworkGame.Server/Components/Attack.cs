using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct Attack : IComponentData
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

    public struct DisableAttack : IComponentData
    {
        
    }
    
    public struct AttackTarget : IComponentData
    {
        public Entity target;
    }
    
    public struct ChaseTarget : IComponentData
    {
        public Entity target;
    }
}