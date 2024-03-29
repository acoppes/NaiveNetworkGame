using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Scenes.Tests
{
    public class TestSendLimitsServer : MonoBehaviour
    {
        private NativeList<NetworkConnection> m_Connections;
        public NetworkDriver m_Driver;

        private NetworkPipeline pipeline;

        [TextArea(5, 15)]
        public string textToSend;

        public bool separatedPackets;
        public bool useFragmentationPipeline;

        public bool useCompression;
        
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

            if (useFragmentationPipeline)
                pipeline = m_Driver.CreatePipeline(typeof(FragmentationPipelineStage));

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
            for (int i = 0; i < m_Connections.Length; i++)
            {
                //Assert.IsTrue(m_Connections[i].IsCreated);

                NetworkEvent.Type cmd;
                while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        var length = stream.ReadUInt();
                        
                        Debug.Log($"Client wants {length} size.");

                        var charArray = textToSend.ToCharArray();

                        var min = math.min(length, charArray.Length);
                        
                        if (separatedPackets)
                        {
                            for (var j = 0; j < min; j++)
                            {
                                var writer = m_Driver.BeginSend(pipeline, m_Connections[i], 1);
                                writer.WriteByte(Convert.ToByte(charArray[j]));
                                m_Driver.EndSend(writer);
                            }
                        } else {

                            if (useCompression)
                            {
                                var bytes = Encoding.UTF8.GetBytes(charArray);
                                byte[] resultBytes;

                                using (var memoryStream = new MemoryStream())
                                {
                                    using (var deflate = new DeflateStream(memoryStream, CompressionLevel.Fastest))
                                    {
                                        deflate.Write(bytes, 0, bytes.Length);
                                    }

                                    resultBytes = memoryStream.ToArray();
                                }
                                
                                var writer = m_Driver.BeginSend(pipeline, m_Connections[i], resultBytes.Length + 2);
                                writer.WriteUShort((ushort) resultBytes.Length);

                                var array = new NativeArray<byte>(resultBytes, Allocator.TempJob);
                                writer.WriteBytes(array);
                                array.Dispose();

                                m_Driver.EndSend(writer);
                            }
                            else
                            {                  
                                var writer = m_Driver.BeginSend(pipeline, m_Connections[i], (int) (min + 2));

                                writer.WriteUShort((ushort) min);

                                for (var j = 0; j < min; j++)
                                {
                                    writer.WriteByte(Convert.ToByte(charArray[j]));
                                }

                                m_Driver.EndSend(writer);
                            }
                        }
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        // Debug.Log("Client disconnected from server");
                        m_Connections[i] = default(NetworkConnection);
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