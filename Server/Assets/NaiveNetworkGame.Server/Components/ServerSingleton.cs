using System;
using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Server.Components
{
    public struct ServerSingleton : ISharedComponentData, IEquatable<ServerSingleton>
    {
        public NetworkManager networkManager;
        public NetworkPipeline framentationPipeline;
        public NetworkPipeline reliabilityPipeline;

        // players needed to start simulation
        public byte playersNeededToStartSimulation;

        public bool Equals(ServerSingleton other)
        {
            return Equals(networkManager, other.networkManager) && 
                   framentationPipeline.Equals(other.framentationPipeline) && 
                   reliabilityPipeline.Equals(other.reliabilityPipeline);
        }

        public override bool Equals(object obj)
        {
            return obj is ServerSingleton other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (networkManager != null ? networkManager.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ framentationPipeline.GetHashCode();
                hashCode = (hashCode * 397) ^ reliabilityPipeline.GetHashCode();
                return hashCode;
            }
        }
    }
}