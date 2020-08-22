using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct LookingDirection : IComponentData
    {
        public float2 direction;
    }
}