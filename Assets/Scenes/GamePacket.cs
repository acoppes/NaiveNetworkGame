using Unity.Mathematics;
using Unity.Networking.Transport;

public struct GamePacket
{
    public static readonly uint CONNECT_COMMAND = 1;
    public static readonly uint MOVE_COMMAND = 2;
    
    public static readonly uint SERVER_ACK = 3;
    
    public static readonly uint GAME_STATE_UPDATE = 4;
    
    public uint type;
    public float2 direction;

    public float2 mainObjectPosition;

    public void Write(ref DataStreamWriter writer)
    {
        writer.WriteUInt(type);
        
        if (type == MOVE_COMMAND)
        {
            writer.WriteFloat(direction.x);
            writer.WriteFloat(direction.y);
        }

        if (type == GAME_STATE_UPDATE)
        {
            writer.WriteFloat(mainObjectPosition.x);
            writer.WriteFloat(mainObjectPosition.y);
        }
    }

    public GamePacket Read(DataStreamReader reader)
    {
        type = reader.ReadUInt();
        
        if (type == MOVE_COMMAND)
        {
            direction.x = reader.ReadFloat();
            direction.y = reader.ReadFloat();
        }

        if (type == GAME_STATE_UPDATE)
        {
            mainObjectPosition.x = reader.ReadFloat();
            mainObjectPosition.y = reader.ReadFloat();
        }
        
        return this;
    }
}