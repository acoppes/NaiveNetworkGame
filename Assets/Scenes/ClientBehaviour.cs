using Unity.Mathematics;
using UnityEngine;

using Unity.Networking.Transport;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool m_Done;

    public GameObject clientObject;
    
    void Start ()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(endpoint);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!m_Done)
                Debug.Log("Something went wrong during connect");
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");

                var packet = new GamePacket
                {
                    type = 1,
                    direction = new float2(0, 0)
                };
                
                // uint value = 1;

                var writer = m_Driver.BeginSend(m_Connection);

                // new GamePacket
                // {
                //     type = GamePacket.CONNECT_COMMAND
                // }.Write(writer);
                
                packet.Write(ref writer);
                
                // writer.WriteUInt(packet.type);
                // writer.WriteFloat(packet.direction.x);
                // writer.WriteFloat(packet.direction.y);

                m_Driver.EndSend(writer);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var packet = new GamePacket().Read(stream);
                
                if (packet.type == GamePacket.SERVER_ACK)
                {
                    Debug.Log("Got ACK from server");
                }
                else if (packet.type == GamePacket.GAME_STATE_UPDATE)
                {
                    clientObject.transform.position = new Vector3(packet.mainObjectPosition.x, 
                        packet.mainObjectPosition.y, 0);
                }
                
                // m_Done = true;
                // m_Connection.Disconnect(m_Driver);
                // m_Connection = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }

        if (m_Connection.IsCreated)
        {
            var move = false;
            var moveVector = float2.zero;
            
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                move = true;
                moveVector.x = -1;
            }
            
            if (Input.GetKey(KeyCode.RightArrow))
            {
                move = true;
                moveVector.x = 1;
            }

            if (move)
            {
                var writer = m_Driver.BeginSend(m_Connection);
           
                new GamePacket
                {
                    type = GamePacket.MOVE_COMMAND,
                    direction = moveVector
                }.Write(ref writer);
                
                // writer.WriteFloat(moveVector.x);
                
                m_Driver.EndSend(writer);
            }
        }
    }
}