using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Server;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace NaiveNetworkGame.Server.Systems
{
    public class UpdateNetworkGameStateSystem : ComponentSystem
    {
        private int frame;
        // private float time;

        protected override void OnCreate()
        {
            base.OnCreate();
            frame = 0;
            // time = 0;

            // TODO: store frame in an entity + singleton component
        }

        protected override void OnUpdate()
        {
            frame++;
            
            // var delta = Time.DeltaTime;
            
            // Entities.WithAll<NetworkGameState>().ForEach(delegate(ref NetworkGameState n)
            // {
            //     n.frame = frame;
            //     // n.delta = delta;
            //     // n.delta = ServerNetworkStaticData.sendGameStateFrequency;
            //     // n.syncVersion = n.version;
            // });
            
            Entities.WithAll<Unit, NetworkGameState>().ForEach(delegate(ref Unit u, 
                ref NetworkGameState n)
            {
                n.unitId = u.id;
                n.playerId = u.player;
                n.unitType = u.type;
            });
            
            Entities
                .WithAll<Unit, Translation, NetworkTranslationSync>()
                .ForEach(delegate(ref Unit unit, ref Translation t, ref NetworkTranslationSync n)
                {
                    n.unitId = unit.id;
                    n.translation = t.Value.xy;
                    n.delta = ServerNetworkStaticData.sendTranslationStateFrequency;
            });
            
            Entities.WithAll<LookingDirection, NetworkGameState>().ForEach(delegate(ref LookingDirection l, 
                ref NetworkGameState n)
            {
                n.lookingDirectionAngleInDegrees = (ushort) 
                    Mathf.RoundToInt(Vector2.Angle(Vector2.right, l.direction));
                // n.lookingDirection = l.direction;
            });
            
            Entities.WithAll<UnitState, NetworkGameState>().ForEach(delegate(ref UnitState state, 
                ref NetworkGameState n)
            {
                n.state = state.state;
                n.statePercentage = state.percentage;
            });
            
            Entities
                .WithAll<PlayerController, NetworkPlayerState, PlayerConnectionId>()
                .ForEach(delegate(ref PlayerController player, ref NetworkPlayerState n, ref PlayerConnectionId p) 
                {
                    n.player = player.player;
                    n.gold = player.gold;
                    n.maxUnits = player.maxUnits;
                    n.currentUnits = player.currentUnits;
                });
        }
    }
}