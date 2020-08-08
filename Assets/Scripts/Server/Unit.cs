using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct Unit : IComponentData
    {
        public int player;
    }
    
    public struct PendingPlayerAction : IComponentData
    {
        public int command;
        public float2 target;
    }
}