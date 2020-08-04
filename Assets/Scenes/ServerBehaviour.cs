using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Networking.Transport;

public class ServerBehaviour : MonoBehaviour
{
    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    public GameObject moveObjectPrefab;
    public GameObject[] clientObjects = new GameObject[20];
    
    public float speed = 1;

    void Start ()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void Update ()
    {
        m_Driver.ScheduleUpdate().Complete();

        // CleanUpConnections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }
        // AcceptNewConnections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");

            var clientObject = Instantiate(moveObjectPrefab, transform);
            clientObject.SetActive(true);

            clientObjects[m_Connections.Length - 1] = clientObject;
        }

        DataStreamReader stream;
        for (var i = 0; i < m_Connections.Length; i++)
        {
            Assert.IsTrue(m_Connections[i].IsCreated);

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                // if (!stream.IsCreated)
                //     continue;
                
                if (cmd == NetworkEvent.Type.Data)
                {
                    // var number = stream.ReadUInt();
                    // var f1 = stream.ReadFloat();
                    // var f2 = stream.ReadFloat();
                    
                    var packet = new GamePacket().Read(stream);

                    if (packet.type == GamePacket.CONNECT_COMMAND)
                    {
                        Debug.Log("New client connected, sending ACK");
                        
                        var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i]);
                        
                        new GamePacket
                        {
                            type = GamePacket.SERVER_CONNECTION_COMPLETED,
                            clientId = (uint) i
                        }.Write(ref writer);
                    
                        m_Driver.EndSend(writer);
                    }
                    
                    if (packet.type == GamePacket.CLIENT_KEEP_ALIVE)
                    {
                        Debug.Log($"Client[{i}] Keep Alive");
                    }
                    
                    if (packet.type == GamePacket.CLIENT_MOVE_COMMAND)
                    {
                        // TODO: move something in the screen
                        Debug.Log("new move command received!!");

                        var clientIndex = (int) packet.clientId;
                        var dir = new float3(packet.direction, 0);
                        var moveObject = clientObjects[clientIndex];
                        
                        moveObject.transform.position += (Vector3) dir * (speed * Time.deltaTime);
                    }
                    
                    // Debug.Log("Got " + number + " from the Client adding + 2 to it.");
                    // number +=2;
                    //
                    // var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i]);
                    // writer.WriteUInt(number);
                    // m_Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
        
        for (var i = 0; i < m_Connections.Length; i++)
        {
            if (m_Connections[i].IsCreated)
            {
                for (var j = 0; j < clientObjects.Length; j++)
                {
                    var clientObject = clientObjects[j];
                    if (clientObject == null)
                        continue;
                    
                    var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i]);
                    
                    new GamePacket
                    {
                        type = GamePacket.SERVER_GAMESTATE_UPDATE,
                        clientId = (uint) i,
                        mainObjectPosition = (Vector2) clientObject.transform.position
                    }.Write(ref writer);
                    
                    m_Driver.EndSend(writer);
                }

            }
        }
    }
}