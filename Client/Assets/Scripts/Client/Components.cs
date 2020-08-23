using Unity.Entities;
using Unity.Mathematics;

namespace Client
{
    public struct PendingPlayerAction : IComponentData
    {
        public uint player;
        public uint command;
        public float2 target;
    }

    public struct ClientOnly : IComponentData
    {
        
    }

    public struct ServerOnly : IComponentData
    {
        
    }
    
    public struct UnitState : IComponentData
    {
        public int state;
        // public float time;
    }
}