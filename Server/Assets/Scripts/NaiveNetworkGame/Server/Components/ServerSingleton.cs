using System;
using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Server.Components
{
    public struct ServerSingleton : ISharedComponentData, IEquatable<ServerSingleton>
    {
        public NetworkManager networkManager;
        // public NetworkPipeline testPipeline;

        public bool Equals(ServerSingleton other)
        {
            return Equals(networkManager, other.networkManager);
        }

        public override bool Equals(object obj)
        {
            return obj is ServerSingleton other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return (networkManager != null ? networkManager.GetHashCode() : 0);
        }
    }
}