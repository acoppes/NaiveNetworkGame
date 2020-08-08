using Unity.Entities;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct Movement : IComponentData
    {
        public float speed;
    }
}