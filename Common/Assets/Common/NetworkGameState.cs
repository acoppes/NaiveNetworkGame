using Unity.Entities;


namespace NaiveNetworkGame.Common
{
    public struct NetworkGameState : IComponentData
    {
        public ushort unitId;
        
        public byte playerId;
        public byte unitType;
        public byte skinType;

        public ushort lookingDirectionAngleInDegrees;
        
        // public float2 lookingDirection;
        
        public byte state;
        public byte statePercentage;

        public byte health;
        
        public static int GetSize()
        {
            unsafe
            {
                return sizeof(NetworkGameState);
            }
        }
        
        public NetworkGameState Write(ref Unity.Collections.DataStreamWriter writer)
        {
            writer.WriteUShort(unitId);
            writer.WriteByte(playerId);
            writer.WriteByte(unitType);
            writer.WriteByte(skinType);
            
            // writer.WriteFloat(lookingDirection.x);
            // writer.WriteFloat(lookingDirection.y);

            writer.WriteUShort(lookingDirectionAngleInDegrees);
            
            writer.WriteByte(state);
            writer.WriteByte(statePercentage);

            writer.WriteByte(health);
            
            return this;
        }

        public NetworkGameState Read(ref Unity.Collections.DataStreamReader stream)
        {
            unitId = stream.ReadUShort();
            playerId = stream.ReadByte();
            unitType = stream.ReadByte();
            skinType = stream.ReadByte();
            
            // lookingDirection.x = stream.ReadFloat();
            // lookingDirection.y = stream.ReadFloat();
            lookingDirectionAngleInDegrees = stream.ReadUShort();
            state = stream.ReadByte();
            statePercentage = stream.ReadByte();
            health = stream.ReadByte();
            return this;
        }
    }
    
}