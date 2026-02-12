using Client;
using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct UpdateUnitModelSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (modelInstance, localTransform) in 
                SystemAPI.Query<ModelInstanceComponent, RefRO<LocalTransform>>()
                    .WithAll<ModelPrefabComponent>())
            {
                modelInstance.instance.transform.position = localTransform.ValueRO.Position;
            }
            
            foreach (var (modelInstance, unitState) in 
                SystemAPI.Query<ModelInstanceComponent, RefRO<UnitStateComponent>>()
                    .WithAll<ModelPrefabComponent>())
            {
                var animator = modelInstance.instance.GetComponent<Animator>();
                if (animator != null)
                    animator.SetInteger("state", unitState.ValueRO.state);

                var model = modelInstance.instance.GetComponent<UnitModelBehaviour>();
                if (model != null)
                {
                    model.isDurationVisible = unitState.ValueRO.state == UnitStateTypes.spawningState;
                    model.durationAlpha = unitState.ValueRO.percentage / 100.0f;
                    // TODO: update with duration alpha from server...
                    // TODO: duration visible should also be defined in server depending the action if
                    // it has duration.
                }
            }
            
            foreach (var (modelInstance, unit) in 
                SystemAPI.Query<ModelInstanceComponent, RefRO<Unit>>())
            {
                var model = modelInstance.instance.GetComponent<UnitModelBehaviour>();
                if (model == null)
                    continue;
                model.isActivePlayer = unit.ValueRO.isLocalPlayer;
                model.isSelected = unit.ValueRO.isSelected;
            }
            
            foreach (var (modelInstance, lookingDirection) in 
                SystemAPI.Query<ModelInstanceComponent, RefRO<LookingDirection>>())
            {
                modelInstance.unitModel.lookingDirection = lookingDirection.ValueRO.direction;
            }
            
            foreach (var (modelInstance, selectable) in 
                SystemAPI.Query<ModelInstanceComponent, RefRW<Selectable>>())
            {
                selectable.ValueRW.bounds = modelInstance.instance.GetComponentInChildren<BoxCollider2D>().bounds;
            }
            
            foreach (var (modelInstance, healthPercentage) in 
                SystemAPI.Query<ModelInstanceComponent, RefRO<HealthPercentage>>())
            {
                modelInstance.unitModel.isHealthBarVisible = healthPercentage.ValueRO.value < 100;
                modelInstance.unitModel.healthBarAlpha = healthPercentage.ValueRO.value / 100.0f;
            }
        }
    }
}
