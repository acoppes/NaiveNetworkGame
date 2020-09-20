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
    }
    
    public struct ClientSingleton : ISharedComponentData, IEquatable<ClientSingleton>
    {
        public NetworkManager networkManager;
        public NetworkPipeline framentationPipeline;
        
        public bool connectionInitialized;
        
        public bool Equals(ClientSingleton other)
        {
            return Equals(networkManager, other.networkManager);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientSingleton other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return (networkManager != null ? networkManager.GetHashCode() : 0);
        }
    }
}