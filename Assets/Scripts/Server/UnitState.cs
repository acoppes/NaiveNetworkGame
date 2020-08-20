using Unity.Entities;

namespace Server
{
    [GenerateAuthoringComponent]
    public struct UnitState : IComponentData
    {
        public int state;
        public float time;
    }
}