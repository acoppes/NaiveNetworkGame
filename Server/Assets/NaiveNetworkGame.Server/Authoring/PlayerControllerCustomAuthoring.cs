using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class PlayerControllerCustomAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        
        [Serializable]
        public struct PlayerActionData
        {
            public byte type;
            public byte cost;
            public GameObject prefab;
        }

        public List<PlayerActionData> actions;
        public Transform buildingSlotsParent;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var buffer = dstManager.AddBuffer<PlayerAction>(entity);
            foreach (var action in actions)
            {
                var prefab = Entity.Null;
                
                if (conversionSystem.HasPrimaryEntity(action.prefab))
                {
                    prefab = conversionSystem.GetPrimaryEntity(action.prefab);
                }
                else
                {
                    prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(action.prefab, new GameObjectConversionSettings
                    {
                        DestinationWorld = dstManager.World
                    });
                }

               //  Assert.IsTrue(conversionSystem.HasPrimaryEntity(action.prefab));

                buffer = dstManager.GetBuffer<PlayerAction>(entity);
                buffer.Add(new PlayerAction
                {
                    type = action.type,
                    cost = action.cost,
                    prefab = prefab
                });
            }

            var buildingSlotsBuffer = dstManager.AddBuffer<BuildingSlot>(entity);
            for (var i = 0; i < buildingSlotsParent.childCount; i++)
            {
                var t = buildingSlotsParent.GetChild(i);
                buildingSlotsBuffer.Add(new BuildingSlot
                {
                    position = new float3(t.position.x, t.position.y, 0),
                    available = true
                });
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (buildingSlotsParent != null)
            {
                for (var i = 0; i < buildingSlotsParent.childCount; i++)
                {
                    var t = buildingSlotsParent.GetChild(i);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(t.position, 0.05f);
                }
            }
        }
    }
}