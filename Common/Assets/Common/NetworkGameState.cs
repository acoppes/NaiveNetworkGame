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
    
    public struct NetworkTranslationSync : IComponentData
    {
        public ushort unitId;
        public float delta;
        public float2 translation;

        public NetworkTranslationSync Write(ref DataStreamWriter writer)
        {
            writer.WriteByte(PacketType.ServerTranslationSync);
            writer.WriteUShort(unitId);
            writer.WriteFloat(delta);
            writer.WriteFloat(translation.x);
            writer.WriteFloat(translation.y);
            return this;
        }

        public NetworkTranslationSync Read(ref DataStreamReader stream)
        {;
            unitId = stream.ReadUShort();
            delta = stream.ReadFloat();
            translation.x = stream.ReadFloat();
            translation.y = stream.ReadFloat();
            return this;
        }
    }
    
    public struct NetworkGameState : IComponentData
    {
        public ushort unitId;
        public byte playerId;
        public byte unitType;

        public ushort lookingDirectionAngleInDegrees;
        
        // public float2 lookingDirection;
        
        public byte state;
        public byte statePercentage;
        
        public NetworkGameState Write(ref DataStreamWriter writer)
        {
            writer.WriteByte(PacketType.ServerGameState);
            writer.WriteUShort(unitId);
            writer.WriteByte(playerId);
            writer.WriteByte(unitType);
            
            // writer.WriteFloat(lookingDirection.x);
            // writer.WriteFloat(lookingDirection.y);

            writer.WriteUShort(lookingDirectionAngleInDegrees);
            
            writer.WriteByte(state);
            writer.WriteByte(statePercentage);
            return this;
        }

        public NetworkGameState Read(ref DataStreamReader stream)
        {
            unitId = stream.ReadUShort();
            playerId = stream.ReadByte();
            unitType = stream.ReadByte();
            // lookingDirection.x = stream.ReadFloat();
            // lookingDirection.y = stream.ReadFloat();
            lookingDirectionAngleInDegrees = stream.ReadUShort();
            state = stream.ReadByte();
            statePercentage = stream.ReadByte();
            return this;
        }
    }
    
}