using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    public struct CreatedUnits : IComponentData
    {
        public int lastCreatedUnitId;
    }

    [GenerateAuthoringComponent]
    public struct Unit : IComponentData
    {
        public ushort id;
        public byte player;
        public byte type;
    }
}