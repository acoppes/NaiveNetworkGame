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
    
    public struct PlayerInputState : IComponentData
    {
        public bool spawnActionPressed;
        public bool selectUnitButtonPressed;
        public bool actionButtonPressed;
        public float2 position;
    }
    
    public struct Unit : IComponentData
    {
        public ushort unitId;
        public byte player;
        public bool isLocalPlayer;
        public bool isSelected;
    }

    public struct PlayerController : IComponentData
    {
        public byte player;
        public ushort gold;
        public byte maxUnits;
        public byte currentUnits;
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