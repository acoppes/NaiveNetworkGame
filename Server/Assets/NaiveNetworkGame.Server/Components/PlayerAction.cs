using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    public struct PlayerAction : IBufferElementData
    {
        public byte type;
        public byte cost;
        public Entity prefab;
    }
}