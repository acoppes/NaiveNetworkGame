using System;
using Unity.Entities;
using Unity.Networking.Transport;

namespace Client
{
    public class ConnectionState
    {
        public enum State
        {
            Connecting,
            WaitingForPlayers,
            SimulationRunning,
            Disconnected
        }

        public static State currentState = State.Connecting;
        public static int totalReceivedBytes;
        public static float connectedTime;
        
        public static float latency;
        public static double latencyPacketLastTime;
        public static byte latencyWaitPacket;
        public static byte latencyPacket;
    }

    public struct ClientSingleton : IComponentData
    {
        
    }

    public struct ClientData: ISharedComponentData, IEquatable<ClientData>
    {
        public NetworkDriver m_Driver;
        
        public NetworkPipeline framentationPipeline;
        public NetworkPipeline reliabilityPipeline;
        public NetworkEndpoint endpoint;
        
        public bool Equals(ClientData other)
        {
            return framentationPipeline.Equals(other.framentationPipeline) && 
                   reliabilityPipeline.Equals(other.reliabilityPipeline) && 
                   endpoint.Equals(other.endpoint);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(framentationPipeline, reliabilityPipeline, endpoint);
        }
    }
}