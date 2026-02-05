using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public struct UnitBehaviourComponent : IComponentData
    {
        public Entity wanderArea;
        public float minIdleTime;
        public float maxIdleTime;
    }
    
    // public class UnitBehaviour : MonoBehaviour, IConvertGameObjectToEntity
    // {
    //     public GameObject wanderArea;
    //     public float minIdleTime;
    //     public float maxIdleTime;
    //     
    //     public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    //     {
    //         dstManager.AddComponentData(entity, new UnitBehaviourComponentData()
    //         {
    //             wanderArea = conversionSystem.GetPrimaryEntity(wanderArea),
    //             minIdleTime = minIdleTime,
    //             maxIdleTime = maxIdleTime
    //         });
    //     }
    // }
}