using Unity.Collections;
using Unity.Networking.Transport;

namespace Client
{
    public class NetworkManager
    {
        public NetworkDriver m_Driver;
        public NativeList<NetworkConnection> m_Connections;
    }
}