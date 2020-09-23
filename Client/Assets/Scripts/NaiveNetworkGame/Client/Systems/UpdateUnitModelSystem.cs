using Client;
using NaiveNetworkGame.Common;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UpdateUnitModelSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ModelPrefabComponent, ModelInstanceComponent, Translation>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref Translation t)
                {
                    m.instance.transform.position = t.Value;
                });
            
            Entities
                .WithAll<ModelPrefabComponent, ModelInstanceComponent, UnitState>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref UnitState state)
                {
                    var animator = m.instance.GetComponent<Animator>();
                    if (animator != null)
                        animator.SetInteger("state", state.state);

                    var model = m.instance.GetComponent<UnitModelBehaviour>();
                    if (model != null)
                    {
                        model.isDurationVisible = state.state == UnitState.spawningState;
                        model.durationAlpha = state.percentage / 100.0f;
                        // TODO: update with duration alpha from server...
                        // TODO: duration visible should also be defined in server depending the action if
                        // it has duration.
                    }
                });
            
            Entities
                .WithAll<ModelPrefabComponent, ModelInstanceComponent, Unit>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref Unit unit)
                {
                    var model = m.instance.GetComponent<UnitModelBehaviour>();
                    if (model == null)
                        return;
                    model.isActivePlayer = unit.isLocalPlayer;
                    model.isSelected = unit.isSelected;
                });
            
            Entities
                .WithAll<ModelInstanceComponent, LookingDirection>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref LookingDirection lookingDirection)
                {
                    m.unitModel.lookingDirection = lookingDirection.direction;
                });
            
            Entities
                .WithAllReadOnly<ModelInstanceComponent>()
                .WithAll<Selectable>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref Selectable selectable)
                {
                    selectable.bounds = m.instance.GetComponentInChildren<BoxCollider2D>().bounds;
                });
            
            Entities
                .WithAll<ModelInstanceComponent, HealthPercentage>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref HealthPercentage h)
                {
                    m.unitModel.isHealthBarVisible = h.value < 100;
                    m.unitModel.healthBarAlpha = h.value / 100.0f;
                });
        }
    }
}