using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Components
{
    public struct IdleAction : IComponentData
    {
        public float time;
    }
    
    public struct AttackAction : IComponentData
    {
        public float time;
    }
    
    public struct MovementAction : IComponentData
    {
        public float2 target;
        public float2 direction;
    }

    public struct SpawningAction : IComponentData
    {
        public float duration;
        public float time;
    }
}