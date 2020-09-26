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
            Connected,
            Disconnected
        }

        public static State currentState = State.Connecting;
        public static int totalReceivedBytes;
        public static float connectedTime;
        
        public static float latency;
    }
    
    public struct ClientSingleton : ISharedComponentData, IEquatable<ClientSingleton>
    {
        public NetworkDriver m_Driver;
        public NetworkPipeline framentationPipeline;
        public NetworkEndPoint endpoint;

        public bool Equals(ClientSingleton other)
        {
            return m_Driver.Equals(other.m_Driver);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientSingleton other && Equals(other);
        }

        public override int GetHashCode()
        {
            return m_Driver.GetHashCode();
        }
    }
}