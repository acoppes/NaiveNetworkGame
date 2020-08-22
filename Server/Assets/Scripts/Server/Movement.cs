using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct Movement : IComponentData
    {
        public float speed;
    }
}