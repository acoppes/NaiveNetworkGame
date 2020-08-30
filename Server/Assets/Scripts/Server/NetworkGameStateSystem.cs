using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Server
{
    public struct NetworkGameState : IComponentData
    {
        public int frame;
        public float delta;

        public int syncVersion;
        public int version;
        
        public int unitId;
        public int playerId;
        
        public float2 translation;
        public float2 lookingDirection;
        public int state;
    }
    
    public class NetworkGameStateSystem : ComponentSystem
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
            var delta = Time.DeltaTime;
            
            Entities.WithAll<NetworkGameState>().ForEach(delegate(ref NetworkGameState n)
            {
                n.frame = frame;
                n.delta = delta;
                n.syncVersion = n.version;
            });
            
            Entities.WithAll<Unit, NetworkGameState>().ForEach(delegate(ref Unit u, 
                ref NetworkGameState n)
            {
                var newUnitId = (int) u.id;
                var newPlayerId = (int) u.player;
                
                if (newUnitId != n.unitId)
                {
                    n.unitId = newUnitId;
                    n.version++;
                }

                if (newPlayerId != n.playerId)
                {
                    n.playerId = newPlayerId;
                    n.version++;
                }
            });
            
            Entities.WithAll<Translation, NetworkGameState>().ForEach(delegate(ref Translation t, 
                ref NetworkGameState n)
            {
                var newTranslation = new float2(t.Value.x, t.Value.y);
                
                if (math.abs(n.translation.x - newTranslation.x) > 0.001f || 
                    math.abs(n.translation.y - newTranslation.y) > 0.001f)
                {
                    n.translation = newTranslation;
                    n.version++;
                }
            });
            
            Entities.WithAll<LookingDirection, NetworkGameState>().ForEach(delegate(ref LookingDirection l, 
                ref NetworkGameState n)
            {
                if (math.distancesq(n.lookingDirection, l.direction) > 0.001f)
                {
                    n.lookingDirection = l.direction;
                    n.version++;
                }
            });
            
            Entities.WithAll<UnitState, NetworkGameState>().ForEach(delegate(ref UnitState state, 
                ref NetworkGameState n)
            {
                if (n.state != state.state)
                {
                    n.state = state.state;
                    n.version++;
                }
            });
        }
    }
}