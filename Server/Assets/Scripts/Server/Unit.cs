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

    public struct ClientOnly : IComponentData
    {
        
    }

    public struct ServerOnly : IComponentData
    {
        
    }
    
}