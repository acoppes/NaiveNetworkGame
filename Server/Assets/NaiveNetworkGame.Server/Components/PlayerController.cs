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
    }

    public struct PlayerBehaviour : IComponentData
    {
        // byte aggressive = 1;
        // byte defensive = 0;
        
        public byte mode;
        // mode data?
    }
}