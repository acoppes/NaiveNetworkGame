using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct ConnectPlayerToServer : IComponentData
    {
        // should be named something like "i want to connect to server"
    }
    
    public class ConnectPlayerToServerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new ConnectPlayerToServer()
            {

            });
        }
    }
}