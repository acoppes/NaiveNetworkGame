using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    public struct PrefabsSharedComponent : ISharedComponentData
    {
        public Entity unitPrefab;
        public Entity treePrefab;
    }
    
    public struct PlayerController : IComponentData
    {
        public int gold;
    }

    public struct IdleAction : IComponentData
    {
        public float time;
    }
    
    public struct MovementAction : IComponentData
    {
        public float2 target;
        public float2 direction;
    }

    public struct SpawningAction : IComponentData
    {
        public float duration;
        public float time;
    }
}