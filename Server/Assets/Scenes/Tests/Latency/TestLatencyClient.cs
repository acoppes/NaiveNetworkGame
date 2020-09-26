using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Tests
{
    public class TestLatencyClient : MonoBehaviour
    {
        public NetworkDriver m_Driver;
        public NetworkConnection m_Connection;

        private NetworkPipeline pipeline;

        public Text latencyText;

        private float latencyUpdateCurrent;
        public float latencyUpdateFrequency;

        private float currentLatencyMs;

        private byte latencyPacketIndex;

        void Start ()
        {
            m_Driver = NetworkDriver.Create(new NetworkDataStreamParameter
            {
                size = 30000
            });
            
            pipeline = NetworkPipeline.Null;

            // if (useFragmentationPipeline)
            //     pipeline = m_Driver.CreatePipeline(typeof(FragmentationPipelineStage));

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
                // Debug.Log("Something went wrong during connect");
                return;
            }

            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Debug.Log("We are now connected to the server");

                    /*var writer = m_Driver.BeginSend(pipeline, m_Connection);
                    writer.WriteUInt(1);
                    m_Driver.EndSend(writer);*/
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    var latencyPacketIndex = stream.ReadByte();
                    var time = stream.ReadFloat();
                    currentLatencyMs = (Time.realtimeSinceStartup - time) * 0.5f;
                    latencyText.text = $"{Mathf.RoundToInt(currentLatencyMs * 1000)}ms";
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    // Debug.Log("Client got disconnected from server");
                    m_Connection = default;
                }
            }

            latencyUpdateCurrent += Time.deltaTime;
            if (latencyUpdateCurrent > latencyUpdateFrequency)
            {
                latencyUpdateCurrent -= latencyUpdateFrequency;
                if (m_Driver.GetConnectionState(m_Connection) == NetworkConnection.State.Connected)
                {
                    var latencyPacket = m_Driver.BeginSend(pipeline, m_Connection);
                    latencyPacket.WriteByte(latencyPacketIndex++);
                    latencyPacket.WriteFloat(Time.realtimeSinceStartup);
                    m_Driver.EndSend(latencyPacket);
                }
            }
        }
    }
}
