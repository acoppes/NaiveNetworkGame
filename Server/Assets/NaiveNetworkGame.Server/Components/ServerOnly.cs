using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public struct ServerOnlyComponentData : IComponentData
    {
        
    }

    public class ServerOnly : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            
        }
    }
}