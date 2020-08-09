using Unity.Mathematics;
using Unity.Networking.Transport;

public struct GamePacket
{
    public static readonly uint CONNECT_COMMAND = 1;
    public static readonly uint CLIENT_MOVE_COMMAND = 2;
    public static readonly uint CLIENT_KEEP_ALIVE = 5;
    
    public static readonly uint SERVER_CONNECTION_COMPLETED = 3;
    public static readonly uint SERVER_GAMESTATE_UPDATE = 4;
    
    public uint type;

    public uint networkPlayerId;
    
    public float2 direction;

    public float2 mainObjectPosition;

    public void Write(ref DataStreamWriter writer)
    {
        writer.WriteUInt(type);
        
        if (type == SERVER_CONNECTION_COMPLETED)
        {
            writer.WriteUInt(networkPlayerId);
        }
        
        if (type == CLIENT_MOVE_COMMAND)
        {
            writer.WriteUInt(networkPlayerId);
            writer.WriteFloat(direction.x);
            writer.WriteFloat(direction.y);
        }

        if (type == SERVER_GAMESTATE_UPDATE)
        {
            writer.WriteUInt(networkPlayerId);
            writer.WriteFloat(mainObjectPosition.x);
            writer.WriteFloat(mainObjectPosition.y);
        }
    }

    public GamePacket Read(DataStreamReader reader)
    {
        type = reader.ReadUInt();

        if (type == SERVER_CONNECTION_COMPLETED)
        {
            networkPlayerId = reader.ReadUInt();
        }
        
        if (type == CLIENT_MOVE_COMMAND)
        {
            networkPlayerId = reader.ReadUInt();
            direction.x = reader.ReadFloat();
            direction.y = reader.ReadFloat();
        }

        if (type == SERVER_GAMESTATE_UPDATE)
        {
            networkPlayerId = reader.ReadUInt();
            mainObjectPosition.x = reader.ReadFloat();
            mainObjectPosition.y = reader.ReadFloat();
        }
        
        return this;
    }
}