using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Server.Components
{
    public struct PlayerConnectionId : IComponentData
    {
        public NetworkConnection connection;
        // public byte player;
        // public bool synchronized;
        // public bool destroyed;
    }
}