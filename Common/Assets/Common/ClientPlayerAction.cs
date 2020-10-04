using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Common
{
    public struct ClientPlayerAction : IComponentData
    {
        public static byte MoveUnitAction = 1;
        public static byte BuildUnit = 2;

        public byte player;
        public uint unit;
        
        public byte actionType;
        public byte unitType;
        
        public float2 target;
        
        public ClientPlayerAction Write(ref DataStreamWriter writer)
        {
            writer.WriteByte(PacketType.ClientPlayerAction);
            writer.WriteByte(player);
            writer.WriteUInt(unit);
            writer.WriteByte(actionType);
            writer.WriteByte(unitType);
            writer.WriteFloat(target.x);
            writer.WriteFloat(target.y);
            return this;
        }

        public ClientPlayerAction Read(ref DataStreamReader stream)
        {
            player = stream.ReadByte();
            unit = stream.ReadUInt();
            actionType = stream.ReadByte();
            unitType = stream.ReadByte();
            target.x = stream.ReadFloat();
            target.y = stream.ReadFloat();
            return this;
        }
    }
}