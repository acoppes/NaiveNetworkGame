using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Common
{
    public struct NetworkGameState : IComponentData
    {
        public ushort unitId;
        public byte playerId;
        public byte unitType;

        public ushort lookingDirectionAngleInDegrees;
        
        // public float2 lookingDirection;
        
        public byte state;
        public byte statePercentage;
        
        public static int GetSize()
        {
            unsafe
            {
                return sizeof(NetworkGameState);
            }
        }
        
        public NetworkGameState Write(ref DataStreamWriter writer)
        {
            // writer.WriteByte(PacketType.ServerGameState);
            
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