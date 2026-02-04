using Unity.Entities;

namespace NaiveNetworkGame.Common
{
    public static class UnitStateTypes
    {
        public static readonly byte idleState = 0;
        public static readonly byte walkState = 1;
        public static readonly byte spawningState = 2;
        public static readonly byte attackingState = 3;
        public static readonly byte reloadingState = 4;
        public static readonly byte deathState = 5;
    }
    
    [GenerateAuthoringComponent]
    public struct UnitState : IComponentData
    {
        public byte state;
        public byte percentage;
    }
}