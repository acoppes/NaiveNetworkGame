using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct PendingAction : IComponentData
    {
        public int command;
        public float2 target;
    }


}