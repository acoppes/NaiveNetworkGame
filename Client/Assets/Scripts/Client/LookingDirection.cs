using Unity.Entities;
using Unity.Mathematics;

namespace Client
{
    public struct LookingDirection : IComponentData
    {
        public float2 direction;
    }
}