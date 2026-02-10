using System;
using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Server.Components
{
    public struct ServerSingleton : IComponentData
    {
        
    }
    
    public struct ServerData : ISharedComponentData, IEquatable<ServerData>
    {
        public bool started;
        public NetworkManager networkManager;
        public NetworkPipeline framentationPipeline;
        public NetworkPipeline reliabilityPipeline;

        // players needed to start simulation
        public ushort port;
        public byte playersNeededToStartSimulation;

        public bool Equals(ServerData other)
        {
            return started == other.started && Equals(networkManager, other.networkManager) && 
                   framentationPipeline.Equals(other.framentationPipeline) && 
                   reliabilityPipeline.Equals(other.reliabilityPipeline) && 
                   playersNeededToStartSimulation == other.playersNeededToStartSimulation;
        }

        public override bool Equals(object obj)
        {
            return obj is ServerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = started.GetHashCode();
                hashCode = (hashCode * 397) ^ (networkManager != null ? networkManager.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ framentationPipeline.GetHashCode();
                hashCode = (hashCode * 397) ^ reliabilityPipeline.GetHashCode();
                hashCode = (hashCode * 397) ^ playersNeededToStartSimulation.GetHashCode();
                return hashCode;
            }
        }
    }
}