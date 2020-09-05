using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Common
{
    public struct ClientPlayerAction : IComponentData
    {
        public static byte MoveUnitAction = 1;

        public byte player;
        public uint unit;
        public byte command;
        public float2 target;
        
        public ClientPlayerAction Write(ref DataStreamWriter writer)
        {
            writer.WriteByte(PacketType.ClientPlayerAction);
            writer.WriteByte(player);
            writer.WriteUInt(unit);
            writer.WriteByte(command);
            writer.WriteFloat(target.x);
            writer.WriteFloat(target.y);
            return this;
        }

        public ClientPlayerAction Read(ref DataStreamReader stream)
        {
            player = stream.ReadByte();
            unit = stream.ReadUInt();
            command = stream.ReadByte();
            target.x = stream.ReadFloat();
            target.y = stream.ReadFloat();
            return this;
        }
    }
}