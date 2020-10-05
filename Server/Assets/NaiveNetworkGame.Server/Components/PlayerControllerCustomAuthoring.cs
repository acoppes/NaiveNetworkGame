using System;
using System.Collections.Generic;
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
                buffer.Add(new PlayerAction
                {
                    type = action.type,
                    cost = action.cost,
                    prefab = conversionSystem.GetPrimaryEntity(action.prefab)
                });
            }
        }
    }
}