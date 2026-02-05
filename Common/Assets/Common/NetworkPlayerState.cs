using Unity.Entities;


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
        public byte freeBarracks;
        
        public byte behaviourMode;
        
        public NetworkPlayerState Write(ref Unity.Collections.DataStreamWriter writer)
        {
            writer.WriteByte(player);
            writer.WriteByte(skinType);
            writer.WriteByte(maxUnits);
            writer.WriteByte(currentUnits);
            writer.WriteUShort(gold);
            writer.WriteByte(buildingSlots);
            writer.WriteByte(freeBarracks);
            writer.WriteByte(behaviourMode);

            return this;
        }

        public NetworkPlayerState Read(ref Unity.Collections.DataStreamReader stream)
        {
            player = stream.ReadByte();
            skinType = stream.ReadByte();
            maxUnits = stream.ReadByte();
            currentUnits = stream.ReadByte();
            gold = stream.ReadUShort();
            buildingSlots = stream.ReadByte();
            freeBarracks = stream.ReadByte();
            behaviourMode = stream.ReadByte();
            return this;
        }
    }
}