using Unity.Entities;
using UnityEngine;


namespace NaiveNetworkGame.Client
{
    public struct PlayerPendingAction : IComponentData
    {
        public bool pending;
        public byte actionType;
        public byte unitType;
    }
    
    public class PlayerPendingActionAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new PlayerPendingAction()
            {

            });
        }
    }
}