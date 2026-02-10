using Client;
using NaiveNetworkGame.Common;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    [UpdateAfter(typeof(CreateUnitFromNetworkGameStateSystem))]
    public partial class UpdateUnitFromNetworkGameStateSystem : SystemBase
    {
        private Vector2 Vector2FromAngle(float a)
        {
            a *= Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }
        
        protected override void OnUpdate()
        {
            // TODO: separate in different network state syncs too

            // updates all created units with network state...
            
            Entities
                .WithAll<NetworkGameState, ClientOnly>()
                .ForEach(delegate(Entity e, ref NetworkGameState n)
                {
                    // var uid = n.unitId;
                    var ngs = n;
                    
                    Entities
                        .WithAll<Unit, LookingDirection, HealthPercentage>()
                        .ForEach(delegate(ref Unit u, ref UnitStateComponent us, ref LookingDirection l, ref HealthPercentage h)
                        {
                            if (u.unitId != ngs.unitId)
                                return;

                            h.value = ngs.health;

                            us.state = ngs.state;
                            us.percentage = ngs.statePercentage;
                            
                            l.direction = Vector2FromAngle(ngs.lookingDirectionAngleInDegrees);
                        });
                
                    PostUpdateCommands.DestroyEntity(e);
                });
        }
    }
}