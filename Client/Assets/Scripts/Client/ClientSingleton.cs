using System;
using NaiveNetworkGame.Common;
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
    
    public struct ClientSingleton : ISharedComponentData, IEquatable<ClientSingleton>
    {
        public NetworkDriver m_Driver;
        public NetworkPipeline framentationPipeline;
        public NetworkPipeline reliabilityPipeline;
        public NetworkEndPoint endpoint;
        
        public bool Equals(ClientSingleton other)
        {
            return m_Driver.Equals(other.m_Driver) && 
                   framentationPipeline.Equals(other.framentationPipeline) && 
                   reliabilityPipeline.Equals(other.reliabilityPipeline) && 
                   endpoint.Equals(other.endpoint);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientSingleton other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Driver.GetHashCode();
                hashCode = (hashCode * 397) ^ framentationPipeline.GetHashCode();
                hashCode = (hashCode * 397) ^ reliabilityPipeline.GetHashCode();
                hashCode = (hashCode * 397) ^ endpoint.GetHashCode();
                return hashCode;
            }
        }
    }
}