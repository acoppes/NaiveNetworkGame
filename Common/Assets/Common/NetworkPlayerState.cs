using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Common
{
    public struct NetworkPlayerState : IComponentData
    {
        public byte player;
        public byte maxUnits;
        public byte currentUnits;
        public ushort gold;
        
        public NetworkPlayerState Write(ref DataStreamWriter writer)
        {
            writer.WriteByte(PacketType.ServerPlayerState);
            writer.WriteByte(player);
            writer.WriteByte(maxUnits);
            writer.WriteByte(currentUnits);
            writer.WriteUShort(gold);
            return this;
        }

        public NetworkPlayerState Read(ref DataStreamReader stream)
        {
            player = stream.ReadByte();
            maxUnits = stream.ReadByte();
            currentUnits = stream.ReadByte();
            gold = stream.ReadUShort();
            return this;
        }
    }
}