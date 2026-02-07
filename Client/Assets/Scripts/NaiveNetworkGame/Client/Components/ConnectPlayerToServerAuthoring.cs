using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct ConnectPlayerToServer : IComponentData
    {
        // should be named something like "i want to connect to server"
    }
    
    public class ConnectPlayerToServerAuthoring : MonoBehaviour
    {
        private class ConnectPlayerToServerBaker : Baker<ConnectPlayerToServerAuthoring>
        {
            public override void Bake(ConnectPlayerToServerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ConnectPlayerToServer());
            }
        }
    }
}