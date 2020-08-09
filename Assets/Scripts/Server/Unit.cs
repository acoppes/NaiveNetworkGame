using System;
using Scenes;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct Unit : IComponentData
    {
        public uint player;
    }
    
    public struct PendingPlayerAction : IComponentData
    {
        public uint player;
        public int command;
        public float2 target;
    }

    public struct ClientOnly : IComponentData
    {
        
    }

    public struct NetworkPlayerId : IComponentData
    {
        public uint player;
    }

    public struct ServerOnly : IComponentData
    {
        
    }
    
}