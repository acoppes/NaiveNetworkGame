using Unity.Entities;
using Unity.Mathematics;


namespace NaiveNetworkGame.Common
{
    public struct PendingPlayerAction : IComponentData
    {
        public byte player;
        
        public byte actionType;
        public byte unitType;
        
        public float2 target;
        
        public PendingPlayerAction Write(ref Unity.Collections.DataStreamWriter writer)
        {
            writer.WriteByte(PacketType.ClientPlayerAction);
            writer.WriteByte(player);
            writer.WriteByte(actionType);
            writer.WriteByte(unitType);
            writer.WriteFloat(target.x);
            writer.WriteFloat(target.y);
            return this;
        }

        public PendingPlayerAction Read(ref Unity.Collections.DataStreamReader stream)
        {
            player = stream.ReadByte();
            actionType = stream.ReadByte();
            unitType = stream.ReadByte();
            target.x = stream.ReadFloat();
            target.y = stream.ReadFloat();
            return this;
        }
    }
}