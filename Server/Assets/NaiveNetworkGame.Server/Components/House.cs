using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct House : IComponentData
    {
        public byte maxUnits;
    }
}