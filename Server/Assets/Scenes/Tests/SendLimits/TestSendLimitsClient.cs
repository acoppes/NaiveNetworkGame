using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;

namespace Scenes.Tests
{
    public class TestSendLimitsClient : MonoBehaviour
    {
        public NetworkDriver m_Driver;
        public NetworkConnection m_Connection;
        public bool m_Done;

        private NetworkPipeline pipeline;
        
        public uint testLength;
        public int receivedLength;

        [TextArea(5, 15)]
        public string receivedText;
        
        public bool separatedPackets;
        
        private StringBuilder longTermStr = new StringBuilder();

        public bool useFragmentationPipeline;
        public bool useCompression;
        
        void Start ()
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
                    // Debug.Log("We are now connected to the server");

                    var writer = m_Driver.BeginSend(pipeline, m_Connection);
                    writer.WriteUInt(testLength);
                    m_Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    // first ushort is data length
                    if (!separatedPackets)
                    {
                        var str = new StringBuilder();
                        var streamLength = stream.Length;
                        receivedLength = stream.ReadUShort();

                        Debug.Log($"Got stream from server: {streamLength}, {receivedLength}");

                        if (useCompression)
                        {
                            var nativeArray = new NativeArray<byte>(stream.Length - 2, Allocator.TempJob);
                            stream.ReadBytes(nativeArray);

                            using (var memoryStream = new MemoryStream(nativeArray.ToArray()))
                            {
                                using (var deflate = new DeflateStream(memoryStream, CompressionMode.Decompress))
                                {
                                    var bytes = new byte[receivedLength];
                                    deflate.Read(bytes, 0, bytes.Length);
                                    str.Append(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
                                }    
                            }
                            

                            nativeArray.Dispose();
                        }
                        else
                        {
                            for (var i = 0; i < receivedLength; i++)
                            {
                                var b = stream.ReadByte();
                                str.Append(Convert.ToChar(b));
                            } 
                        }

                        Debug.Log(str.ToString());
                        receivedText = str.ToString();
                    
                        m_Done = true;
                        m_Connection.Disconnect(m_Driver);
                        m_Connection = default(NetworkConnection);
                    }
                    else
                    {
                        var b = stream.ReadByte();
                        longTermStr.Append(Convert.ToChar(b));
                        receivedText = longTermStr.ToString();
                        receivedLength = longTermStr.Length;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    // Debug.Log("Client got disconnected from server");
                    m_Connection = default(NetworkConnection);
                }
            }
        }
    }
}
