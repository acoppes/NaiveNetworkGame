using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct PlayerController : IComponentData
    {
        public byte player;
        public byte maxUnits;
        public byte currentUnits;

        public ushort maxGold;
        public ushort gold;

        public byte skinType;

        public Entity defendArea;
        public Entity attackArea;

        public byte availableBuildingSlots;

        public byte freeBarracksCount;

        public float defensiveRange;
    }

    public struct PlayerBehaviour : IComponentData
    {
        public const byte aggressive = 1;
        public const byte defensive = 0;
        
        public byte mode;
        // mode data?
    }
}