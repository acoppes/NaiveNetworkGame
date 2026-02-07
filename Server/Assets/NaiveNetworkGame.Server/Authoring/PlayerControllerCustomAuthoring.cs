using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class PlayerControllerCustomAuthoring : MonoBehaviour
    {
        
        [Serializable]
        public struct PlayerActionData
        {
            public byte type;
            public byte cost;
            public GameObject prefab;
        }

        public List<PlayerActionData> actions;

        public float defensiveRange;

        private class PlayerControllerCustomBaker : Baker<PlayerControllerCustomAuthoring>
        {
            public override void Bake(PlayerControllerCustomAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<PlayerAction>(entity);
            
                foreach (var action in authoring.actions)
                {
                    //  Assert.IsTrue(conversionSystem.HasPrimaryEntity(action.prefab));
                    // buffer = dstManager.GetBuffer<PlayerAction>(entity);
                    
                    buffer.Add(new PlayerAction
                    {
                        type = action.type,
                        cost = action.cost,
                        prefab = action.prefab ? GetEntity(action.prefab, TransformUsageFlags.Dynamic) : Entity.Null
                    });
                }

                AddComponent(entity, new PlayerBehaviour());
                
                // THIS WILL OVERRIDE PLAYER CONTROLLER
                // AddComponent(entity, new PlayerController()
                // {
                //     defensiveRange = authoring.defensiveRange
                // });
                

                // var p = dstManager.GetComponentData<PlayerController>(entity);
                // p.defensiveRange = defensiveRange;
                // dstManager.SetComponentData(entity, p);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, defensiveRange);
        }


    }
}