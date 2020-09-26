using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;

namespace Scenes.Tests
{
    public class TestLatencyServer : MonoBehaviour
    {
        private NativeList<NetworkConnection> m_Connections;
        public NetworkDriver m_Driver;

        private NetworkPipeline pipeline;

        void Start()
        {
            m_Driver = NetworkDriver.Create(new NetworkDataStreamParameter
            {
                size = 30000
            },   new FragmentationUtility.Parameters
            {
                PayloadCapacity = 16 * 1024
            });
            
            pipeline = NetworkPipeline.Null;

            // if (useFragmentationPipeline)
            //     pipeline = m_Driver.CreatePipeline(typeof(FragmentationPipelineStage));

            var endpoint = NetworkEndPoint.AnyIpv4;
            endpoint.Port = 9000;
            if (m_Driver.Bind(endpoint) != 0)
                Debug.Log("Failed to bind to port 9000");
            else
                m_Driver.Listen();

            m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        }

        void Update()
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
                // Debug.Log("Accepted a connection");
            }

            DataStreamReader stream;
            for (var i = 0; i < m_Connections.Length; i++)
            {
                //Assert.IsTrue(m_Connections[i].IsCreated);

                NetworkEvent.Type cmd;
                while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        var latencyPacketIndex = stream.ReadByte();
                        var timeInClient = stream.ReadFloat();

                        var latencyPacketAck = m_Driver.BeginSend(pipeline, m_Connections[i]);
                        latencyPacketAck.WriteByte(latencyPacketIndex);
                        latencyPacketAck.WriteFloat(timeInClient);

                        m_Driver.EndSend(latencyPacketAck);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        // Debug.Log("Client disconnected from server");
                        m_Connections[i] = default;
                    }
                }
            }
        }

        public void OnDestroy()
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }
}