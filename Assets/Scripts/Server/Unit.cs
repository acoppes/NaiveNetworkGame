using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct Unit : IComponentData
    {
        public uint id;
        public uint player;
    }
    
    public struct PendingPlayerAction : IComponentData
    {
        public uint player;
        public uint command;
        public float2 target;
    }

    public struct ClientOnly : IComponentData
    {
        
    }

    public struct NetworkPlayerId : IComponentData
    {
        public uint player;
    }

    public struct ServerOnly : IComponentData
    {
        
    }
    
}