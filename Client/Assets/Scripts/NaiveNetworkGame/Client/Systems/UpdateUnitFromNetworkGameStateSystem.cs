using Client;
using NaiveNetworkGame.Common;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateAfter(typeof(CreateUnitFromNetworkGameStateSystem))]
    public partial struct UpdateUnitFromNetworkGameStateSystem : ISystem
    {
        private Vector2 Vector2FromAngle(float a)
        {
            a *= Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }
        
        public void OnUpdate(ref SystemState state)
        {
            // TODO: separate in different network state syncs too

            // updates all created units with network state...
            
            foreach (var (networkGameState, entity) in 
                SystemAPI.Query<RefRO<NetworkGameState>>()
                    .WithAll<ClientOnly>()
                    .WithEntityAccess())
            {
                var ngs = networkGameState.ValueRO;
                
                foreach (var (unit, unitState, lookingDirection, healthPercentage) in 
                    SystemAPI.Query<RefRO<Unit>, RefRW<UnitStateComponent>, RefRW<LookingDirection>, RefRW<HealthPercentage>>())
                {
                    if (unit.ValueRO.unitId != ngs.unitId)
                        continue;

                    healthPercentage.ValueRW.value = ngs.health;

                    unitState.ValueRW.state = ngs.state;
                    unitState.ValueRW.percentage = ngs.statePercentage;
                    
                    lookingDirection.ValueRW.direction = Vector2FromAngle(ngs.lookingDirectionAngleInDegrees);
                }
            
                state.EntityManager.DestroyEntity(entity);
            }
        }
    }
}
