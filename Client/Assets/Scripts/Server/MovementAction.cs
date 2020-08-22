using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    public struct MovementAction : IComponentData
    {
        public float2 target;
        public float2 direction;
    }
}