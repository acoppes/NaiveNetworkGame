using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct UnitBehaviour : IComponentData
    {
        public Entity wanderArea;
        public float minIdleTime;
        public float maxIdleTime;
    }
}