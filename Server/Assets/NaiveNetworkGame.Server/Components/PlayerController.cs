using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct PlayerController : IComponentData
    {
        public byte player;
        public byte maxUnits;
        public byte currentUnits;
        public ushort gold;

        public byte skinType;

        public Entity unitPrefab;
        public Entity farmPrefab;
        
        public Entity playerWander;
    }
}