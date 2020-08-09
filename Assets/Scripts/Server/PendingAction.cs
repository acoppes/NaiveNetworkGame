using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct PendingAction : IComponentData
    {
        public uint command;
        public float2 target;
    }


}