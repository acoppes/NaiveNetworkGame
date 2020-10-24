using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateAfter(typeof(ProcessPendingPlayerActionsSystem))]
    [UpdateBefore(typeof(UnitBehaviourSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class PlayerBehaviourSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            InitEntityQueryCache(100);
        }

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
                        .ForEach(delegate(Entity ue, ref Unit u, ref UnitBehaviour ub)
                        {
                            if (u.player != player)
                                return;
                            ub.wanderArea = area;
                        });
                        
                    Entities.WithAll<Unit, IdleAction>()
                        .ForEach(delegate(Entity ue, ref Unit u)
                        {
                            if (u.player != player)
                                return;
                            PostUpdateCommands.RemoveComponent<IdleAction>(ue);
                        });
                        
                    Entities.WithAll<Unit, MovementAction>()
                        .ForEach(delegate(Entity ue, ref Unit u)
                        {
                            if (u.player != player)
                                return;
                            PostUpdateCommands.RemoveComponent<MovementAction>(ue);
                        });
                });
            
            Entities
                .WithAll<ServerOnly>()
                .ForEach(delegate(Entity e, ref PlayerController playerController, ref PlayerBehaviour b)
                {
                    var player = playerController.player;

                    switch (b.mode)
                    {
                        case PlayerBehaviour.aggressive:
                            Entities
                                .WithAll<Unit, DisableAttack>()
                                .ForEach(delegate(Entity ue, ref Unit u)
                                {
                                    if (u.player != player)
                                        return;
                                    PostUpdateCommands.RemoveComponent<DisableAttack>(ue);
                                });
                            break;
                        case PlayerBehaviour.defensive:
                            // could be processed all the time, not only here...
                            Entities
                                .WithNone<DisableAttack>()
                                .WithAll<Unit>()
                                .ForEach(delegate(Entity ue, ref Unit u)
                                {
                                    if (u.player != player)
                                        return;
                                    
                                    // disable attack!!
                                    PostUpdateCommands.AddComponent<DisableAttack>(ue);
                                });
                            
                            Entities
                                .WithAll<ChaseTarget, MovementAction, DisableAttack>()
                                .ForEach(delegate(Entity e)
                                {
                                    PostUpdateCommands.RemoveComponent<MovementAction>(e);
                                });
                            
                            Entities
                                .WithAll<ChaseTarget, DisableAttack>()
                                .ForEach(delegate(Entity e)
                                {
                                    PostUpdateCommands.RemoveComponent<ChaseTarget>(e);
                                });

                            Entities
                                .WithAll<AttackTarget, DisableAttack>()
                                .ForEach(delegate(Entity e)
                                {
                                    PostUpdateCommands.RemoveComponent<AttackTarget>(e);
                                });
                            
                            break;
                    }
                });
            
            // Enable attack again for units in defend area...
        }
    }
}