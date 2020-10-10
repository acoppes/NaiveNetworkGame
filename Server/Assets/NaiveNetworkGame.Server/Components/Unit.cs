using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    public static class NetworkUnitId
    {
        public static ushort current = 1;
    }

    public struct Unit : IComponentData
    {
        public ushort id;
        public byte player;
        public byte type;
        public byte slotCost;
    }
}