using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Common
{
    public struct NetworkPlayerState : IComponentData
    {
        public byte player;
        public ushort gold;
        
        public NetworkPlayerState Write(ref DataStreamWriter writer)
        {
            writer.WriteByte(PacketType.ServerPlayerState);
            writer.WriteByte(player);
            writer.WriteUShort(gold);
            return this;
        }

        public NetworkPlayerState Read(ref DataStreamReader stream)
        {
            player = stream.ReadByte();
            gold = stream.ReadUShort();
            return this;
        }
    }
}