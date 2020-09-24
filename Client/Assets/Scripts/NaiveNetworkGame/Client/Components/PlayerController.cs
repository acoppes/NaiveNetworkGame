using Unity.Entities;

namespace NaiveNetworkGame.Client.Components
{
    [GenerateAuthoringComponent]
    public struct PlayerController : IComponentData
    {
        public byte player;
        public byte unitType;
        public ushort gold;
        public byte maxUnits;
        public byte currentUnits;
    }
}