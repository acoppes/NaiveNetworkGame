using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct Attack : IComponentData
    {
        public float damage;
        
        public float range;
        
        public float duration;
        // public float chaseRange;
    }

    public struct AttackTarget : IComponentData
    {
        public Entity target;
    }
}