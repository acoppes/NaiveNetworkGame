using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Components
{
    public struct UnitBehaviour : IComponentData
    {
        public float2 wanderCenter;
        public float range;
    }
    
    public struct IdleAction : IComponentData
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