using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    public struct CreatedUnits : IComponentData
    {
        public int lastCreatedUnitId;
    }
    
    [GenerateAuthoringComponent]
    public struct Unit : IComponentData
    {
        public uint id;
        public uint player;
        public byte type;
    }
    
    public struct PendingPlayerAction : IComponentData
    {
        public uint player;
        public uint unit;
        public uint command;
        public float2 target;
    }

    public struct ClientOnly : IComponentData
    {
        
    }

    public struct ServerOnly : IComponentData
    {
        
    }
    
}