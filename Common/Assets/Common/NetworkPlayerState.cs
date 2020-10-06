using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Common
{
    public struct NetworkPlayerState : IComponentData
    {
        public byte player;
        public byte skinType;
        public byte maxUnits;
        public byte currentUnits;
        public ushort gold;
        public byte buildingSlots;
        
        public NetworkPlayerState Write(ref DataStreamWriter writer)
        {
            writer.WriteByte(player);
            writer.WriteByte(skinType);
            writer.WriteByte(maxUnits);
            writer.WriteByte(currentUnits);
            writer.WriteUShort(gold);
            writer.WriteByte(buildingSlots);
            return this;
        }

        public NetworkPlayerState Read(ref DataStreamReader stream)
        {
            player = stream.ReadByte();
            skinType = stream.ReadByte();
            maxUnits = stream.ReadByte();
            currentUnits = stream.ReadByte();
            gold = stream.ReadUShort();
            buildingSlots = stream.ReadByte();
            return this;
        }
    }
}