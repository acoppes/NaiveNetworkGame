using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public struct ServerOnly : IComponentData
    {
        
    }

    public class ServerOnlyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            
        }
    }
}