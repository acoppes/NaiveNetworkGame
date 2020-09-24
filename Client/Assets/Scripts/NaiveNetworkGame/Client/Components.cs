using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;

namespace NaiveNetworkGame.Client
{
    // public struct ClientConnectionId : IComponentData
    // {
    //     public uint id;
    // }

    public struct Unit : IComponentData
    {
        public ushort unitId;
        public byte player;
        public bool isLocalPlayer;
        public bool isSelected;
    }

    public struct HealthPercentage : IComponentData
    {
        public byte value;
    }

    public struct NetworkPlayerId : IComponentData
    {
        public NetworkConnection connection;
        public NetworkConnection.State state;
    }

    public struct NetworkObject : IComponentData
    {
        
    }
    
    public struct ConfirmActionFeedback : IComponentData
    {
        public float2 position;
    }
    
    public struct TranslationInterpolation : IComponentData
    {
        public float time;
        public float alpha;

        public float2 previousTranslation;
        public float2 currentTranslation;

        public float remoteDelta;
    }
    
    public struct Selectable : IComponentData
    {
        public Bounds bounds;
    }

}