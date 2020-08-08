using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Server
{
    public class PendingPlayerActionsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<PendingPlayerAction>().ForEach(delegate (Entity e, ref PendingPlayerAction p)
            {
                PostUpdateCommands.DestroyEntity(e);

                var player = p.player;
                
                var pendingAction = new PendingAction
                {
                    command = p.command,
                    target = p.target
                };
                
                Entities.WithAll<Unit, Movement>().ForEach(delegate(Entity unitEntity, ref Unit unit)
                {
                    if (unit.player != player) 
                        return;
                    
                    PostUpdateCommands.RemoveComponent<PendingAction>(unitEntity);
                    PostUpdateCommands.RemoveComponent<MovementAction>(unitEntity);
                    PostUpdateCommands.AddComponent(unitEntity, pendingAction);
                });
            });
        }
    }
    
    public class PendingActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<PendingAction, Movement>().WithNone<MovementAction>().ForEach(delegate (Entity e, ref PendingAction p)
            {
                PostUpdateCommands.RemoveComponent<PendingAction>(e);
                
                // movement command
                if (p.command == 0)
                {
                    PostUpdateCommands.AddComponent(e, new MovementAction
                    {
                        target = p.target
                    });
                }
            });
        }
    }

    public class MovementSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            
            Entities.WithAll<Movement, Translation>().WithAllReadOnly<MovementAction>()
                .ForEach(delegate(Entity e, ref Movement movement, ref Translation t, ref MovementAction m)
                {
                    var p0 = t.Value.xy;
                    var p1 = m.target;

                    var direction = math.normalize(p1 - p0);
                    
                    var newpos = p0 +  direction * movement.speed * dt;

                    if (math.distancesq(p1, p0) < math.distancesq(newpos, p0))
                    {
                        newpos = p1;
                        PostUpdateCommands.RemoveComponent<MovementAction>(e);
                    }

                    t.Value = new float3(newpos.x, newpos.y, t.Value.z);
                });
        }
    }
}