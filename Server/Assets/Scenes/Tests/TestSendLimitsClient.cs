using System;
using System.Text;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Tests
{
    public class TestSendLimitsClient : MonoBehaviour
    {
        public NetworkDriver m_Driver;
        public NetworkConnection m_Connection;
        public bool m_Done;

        public uint testLength;

        [TextArea(5, 15)]
        public string receivedText;

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
                    // Debug.Log("We are now connected to the server");

                    var writer = m_Driver.BeginSend(m_Connection);
                    writer.WriteUInt(testLength);
                    m_Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    // first ushort is data length
                    var streamLength = stream.Length;
                    var receivedLength = stream.ReadUShort();
                    var str = new StringBuilder();
                 
                    Debug.Log($"Got stream from server: {streamLength}, {receivedLength}");
                    
                    for (var i = 0; i < receivedLength; i++)
                    {
                        var b = stream.ReadByte();
                        str.Append(Convert.ToChar(b));
                    }

                    Debug.Log(str.ToString());
                    receivedText = str.ToString();
                    
                    m_Done = true;
                    m_Connection.Disconnect(m_Driver);
                    m_Connection = default(NetworkConnection);
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
