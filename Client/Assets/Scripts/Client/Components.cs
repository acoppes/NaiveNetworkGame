using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;

namespace Client
{
    // public struct ClientConnectionId : IComponentData
    // {
    //     public uint id;
    // }

    public struct ClientOnly : IComponentData
    {
        
    }

    public struct ServerOnly : IComponentData
    {
        
    }

    public struct PlayerInputState : IComponentData
    {
        public bool spawnActionPressed;
        public bool selectUnitButtonPressed;
        public bool actionButtonPressed;
    }
    
    public struct UnitComponent : IComponentData
    {
        public uint unitId;
        public uint player;
        public bool isActivePlayer;
        public bool isSelected;
    }

    public struct NetworkPlayerId : IComponentData
    {
        public byte player;
        public NetworkConnection connection;
    }

    public struct ConfirmActionFeedback : IComponentData
    {
        public float2 position;
    }
}