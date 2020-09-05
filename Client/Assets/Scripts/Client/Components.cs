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
        public bool spawnWaitingForPosition;
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
    
    public struct UnitState : IComponentData
    {
        public int state;
        // public float time;
    }
    
    public struct NetworkPlayerId : IComponentData
    {
        public uint player;
        public NetworkConnection connection;
    }

    public struct ConfirmActionFeedback : IComponentData
    {
        public float2 position;
    }
}