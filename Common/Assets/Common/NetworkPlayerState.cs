using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Common
{
    public struct NetworkPlayerState : IComponentData
    {
        public byte player;
        public byte unitType;
        public byte maxUnits;
        public byte currentUnits;
        public ushort gold;
        
        public NetworkPlayerState Write(ref DataStreamWriter writer)
        {
            writer.WriteByte(player);
            writer.WriteByte(unitType);
            writer.WriteByte(maxUnits);
            writer.WriteByte(currentUnits);
            writer.WriteUShort(gold);
            return this;
        }

        public NetworkPlayerState Read(ref DataStreamReader stream)
        {
            player = stream.ReadByte();
            unitType = stream.ReadByte();
            maxUnits = stream.ReadByte();
            currentUnits = stream.ReadByte();
            gold = stream.ReadUShort();
            return this;
        }
    }
}