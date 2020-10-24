using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ProcessPendingPlayerActionsSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class PlayerBehaviourSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ServerOnly>()
                .ForEach(delegate(Entity e, ref SwitchToAttackAction a, ref PlayerController playerController, 
                    ref PlayerBehaviour b)
            {
                PostUpdateCommands.RemoveComponent<SwitchToAttackAction>(e);
                
                var player = playerController.player;
                var area = playerController.attackArea;

                b.mode = PlayerBehaviour.aggressive;
                
                Entities.WithAll<Unit, UnitBehaviour>()
                    .ForEach(delegate(Entity unitEntity, ref Unit u, ref UnitBehaviour ub)
                    {
                        if (u.player != player)
                            return;
                        ub.wanderArea = area;
                    });
                        
                Entities.WithAll<Unit, IdleAction>()
                    .ForEach(delegate(Entity unitEntity, ref Unit u)
                    {
                        if (u.player != player)
                            return;
                        PostUpdateCommands.RemoveComponent<IdleAction>(unitEntity);
                    });
                        
                Entities.WithAll<Unit, MovementAction>()
                    .ForEach(delegate(Entity unitEntity, ref Unit u)
                    {
                        if (u.player != player)
                            return;
                        PostUpdateCommands.RemoveComponent<MovementAction>(unitEntity);
                    });
                
            });
            
            Entities
                .WithAll<ServerOnly>()
                .ForEach(delegate(Entity e, ref SwitchToDefendAction a, ref PlayerController playerController, 
                    ref PlayerBehaviour b)
                {
                    PostUpdateCommands.RemoveComponent<SwitchToDefendAction>(e);
                    
                    var player = playerController.player;
                    var area = playerController.defendArea;

                    b.mode = PlayerBehaviour.defensive;
                
                    Entities.WithAll<Unit, UnitBehaviour>()
                        .ForEach(delegate(Entity unitEntity, ref Unit u, ref UnitBehaviour ub)
                        {
                            if (u.player != player)
                                return;
                            ub.wanderArea = area;
                        });
                        
                    Entities.WithAll<Unit, IdleAction>()
                        .ForEach(delegate(Entity unitEntity, ref Unit u)
                        {
                            if (u.player != player)
                                return;
                            PostUpdateCommands.RemoveComponent<IdleAction>(unitEntity);
                        });
                        
                    Entities.WithAll<Unit, MovementAction>()
                        .ForEach(delegate(Entity unitEntity, ref Unit u)
                        {
                            if (u.player != player)
                                return;
                            PostUpdateCommands.RemoveComponent<MovementAction>(unitEntity);
                        });
                
                });
        }
    }
}