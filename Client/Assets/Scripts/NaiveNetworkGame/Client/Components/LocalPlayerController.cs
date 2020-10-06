using Unity.Entities;

namespace NaiveNetworkGame.Client.Components
{
    [GenerateAuthoringComponent]
    public struct LocalPlayerController : IComponentData
    {
        public byte player;
        public byte skinType;
        public ushort gold;
        public byte maxUnits;
        public byte currentUnits;
        public byte buildingSlots;
    }
}