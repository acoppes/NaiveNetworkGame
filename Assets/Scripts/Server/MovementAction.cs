using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct MovementAction : IComponentData
    {
        public float2 target;
    }
}