using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct Movement : IComponentData
    {
        public float speed;
    }
}