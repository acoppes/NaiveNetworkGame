using Unity.Entities;

namespace NaiveNetworkGame.Client.Components
{
    [GenerateAuthoringComponent]
    public struct LocalPlayer : IComponentData
    {
        public int connectionIndex;
    }
}