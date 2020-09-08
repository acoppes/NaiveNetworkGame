using Unity.Entities;

namespace NaiveNetworkGame.Common
{
    [GenerateAuthoringComponent]
    public struct UnitState : IComponentData
    {
        public static readonly byte idleState = 0;
        public static readonly byte walkState = 1;
        public static readonly byte spawningState = 2;
        
        public byte state;
        public byte percentage;
    }
}