using Unity.Entities;

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
}