using Unity.Entities;
using Unity.Mathematics;
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
    
    public struct NetworkGameState : IComponentData
    {
        public int frame;
        public float delta;
        public int unitId;
        public int playerId;
        public byte unitType;
        public float2 translation;
        public float2 lookingDirection;
        
        public byte state;
        public byte statePercentage;
        
        public NetworkGameState Write(ref DataStreamWriter writer)
        {
            writer.WriteByte(PacketType.ServerGameState);
            writer.WriteInt(frame);
            writer.WriteFloat(delta);
            writer.WriteUInt((uint) unitId);
            writer.WriteByte((byte) playerId);
            writer.WriteByte(unitType);
            writer.WriteFloat(translation.x);
            writer.WriteFloat(translation.y);
            writer.WriteFloat(lookingDirection.x);
            writer.WriteFloat(lookingDirection.y);
            writer.WriteByte(state);
            writer.WriteByte(statePercentage);
            return this;
        }

        public NetworkGameState Read(ref DataStreamReader stream)
        {
            frame = stream.ReadInt();
            delta = stream.ReadFloat();
            unitId = (int) stream.ReadUInt();
            playerId = stream.ReadByte();
            unitType = stream.ReadByte();
            translation.x = stream.ReadFloat();
            translation.y = stream.ReadFloat();
            lookingDirection.x = stream.ReadFloat();
            lookingDirection.y = stream.ReadFloat();
            state = stream.ReadByte();
            statePercentage = stream.ReadByte();
            return this;
        }
    }
    
}