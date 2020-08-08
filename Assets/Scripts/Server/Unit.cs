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
        public int player;
    }
    
    public struct PendingPlayerAction : IComponentData
    {
        public int player;
        public int command;
        public float2 target;
    }

    public struct ClientOnly : IComponentData
    {
        
    }

    public struct ServerOnly : IComponentData
    {
        
    }
    
}