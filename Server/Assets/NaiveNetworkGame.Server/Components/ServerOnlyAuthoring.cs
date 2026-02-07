using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public struct ServerOnly : IComponentData
    {
        
    }

    public class ServerOnlyAuthoring : MonoBehaviour
    {
        private class ServerOnlyBaker : Baker<ServerOnlyAuthoring>
        {
            public override void Bake(ServerOnlyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ServerOnly());
            }
        }
    }
}