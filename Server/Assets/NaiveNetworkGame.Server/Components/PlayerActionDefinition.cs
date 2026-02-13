using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    public struct PlayerActionDefinition : IBufferElementData
    {
        public static byte BuildUnit = 2;
        public static byte Attack = 3;
        public static byte Defend = 4;
        
        public byte type;
        public byte cost;
        public Entity prefab;
    }
}