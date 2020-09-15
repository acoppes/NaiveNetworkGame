using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Server
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
            
            Entities.WithAll<NetworkGameState>().ForEach(delegate(ref NetworkGameState n)
            {
                n.frame = frame;
                // n.delta = delta;
                n.delta = ServerNetworkStaticData.sendGameStateFrequency;
                // n.syncVersion = n.version;
            });
            
            Entities.WithAll<Unit, NetworkGameState>().ForEach(delegate(ref Unit u, 
                ref NetworkGameState n)
            {
                n.unitId = (int) u.id;
                n.playerId = u.player;
                n.unitType = u.type;
            });
            
            Entities
                .WithAll<Unit, Translation, NetworkTranslationSync>()
                .ForEach(delegate(ref Unit unit, ref Translation t, ref NetworkTranslationSync n)
            {
                var newTranslation = new float2(t.Value.x, t.Value.y);
                n.translation = newTranslation;
                n.delta = ServerNetworkStaticData.sendTranslationStateFrequency;
            });
            
            // Entities.WithAll<LookingDirection, NetworkGameState>().ForEach(delegate(ref LookingDirection l, 
            //     ref NetworkGameState n)
            // {
            //     n.lookingDirection = l.direction;
            // });
            
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
            });
        }
    }
}