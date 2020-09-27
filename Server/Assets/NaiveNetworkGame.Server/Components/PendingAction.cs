using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct PendingAction : IComponentData
    {
        public uint command;
        public float2 target;
    }


}