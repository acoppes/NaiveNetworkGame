using Unity.Mathematics;
using Unity.Networking.Transport;

public struct GamePacket
{
    public static readonly uint CONNECT_COMMAND = 1;
    public static readonly uint MOVE_COMMAND = 2;
    
    public static readonly uint SERVER_ACK = 3;
    
    public uint type;
    public float2 direction;

    public void Write(ref DataStreamWriter writer)
    {
        writer.WriteUInt(type);
        writer.WriteFloat(direction.x);
        writer.WriteFloat(direction.y);
    }

    public GamePacket Read(DataStreamReader reader)
    {
        type = reader.ReadUInt();
        
        if (type == MOVE_COMMAND)
        {
            direction.x = reader.ReadFloat();
            direction.y = reader.ReadFloat();
        }
        
        return this;
    }
}