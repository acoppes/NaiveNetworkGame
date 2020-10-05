using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Entities;
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
        }
    }
}