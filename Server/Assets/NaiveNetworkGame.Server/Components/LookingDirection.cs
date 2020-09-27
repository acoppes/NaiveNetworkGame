using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct LookingDirection : IComponentData
    {
        public float2 direction;
    }
}