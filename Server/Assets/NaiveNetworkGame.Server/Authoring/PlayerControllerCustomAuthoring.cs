using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class PlayerControllerCustomAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        
        [Serializable]
        public struct PlayerActionData
        {
            public byte type;
            public byte cost;
            public GameObject prefab;
        }

        public List<PlayerActionData> actions;
        // public Transform buildingSlotsParent;

        public float defensiveRange;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            foreach (var action in actions)
            {
                if (action.prefab != null)
                    referencedPrefabs.Add(action.prefab);
            }
        }
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var buffer = dstManager.AddBuffer<PlayerAction>(entity);
            
            foreach (var action in actions)
            {
                //  Assert.IsTrue(conversionSystem.HasPrimaryEntity(action.prefab));

                buffer = dstManager.GetBuffer<PlayerAction>(entity);
                buffer.Add(new PlayerAction
                {
                    type = action.type,
                    cost = action.cost,
                    prefab = action.prefab != null ? conversionSystem.GetPrimaryEntity(action.prefab) : Entity.Null
                });
            }

            dstManager.AddComponentData(entity, new PlayerBehaviour());

            var p = dstManager.GetComponentData<PlayerController>(entity);
            p.defensiveRange = defensiveRange;
            dstManager.SetComponentData(entity, p);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, defensiveRange);
        }


    }
}