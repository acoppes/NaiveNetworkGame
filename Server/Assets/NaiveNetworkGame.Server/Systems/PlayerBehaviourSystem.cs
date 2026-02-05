using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    [UpdateBefore(typeof(PlayerBehaviourSystem))]
    public class UpdateChaseCenterSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // By default, we sue the unit position as the chase center
            Entities
                .WithAll<AttackComponent, Translation, IsAlive>()
                .ForEach(delegate(Entity e, ref AttackComponent attack, ref Translation t)
                {
                    attack.chaseCenter = t.Value;
                });
        }
    }
    
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
                .ForEach(delegate(Entity e, ref PlayerController playerController, ref Translation t, ref PlayerBehaviour b)
                {
                    var player = playerController.player;
                    var defendCenter = t.Value;
                    var defendRange = playerController.defensiveRange * playerController.defensiveRange;

                    var attackArea = playerController.attackArea;
                    var defendArea = playerController.defendArea;

                    switch (b.mode)
                    {
                        case PlayerBehaviour.aggressive:
                            
                            Entities.WithAll<Unit, UnitBehaviourComponent>()
                                .ForEach(delegate(Entity unitEntity, ref Unit u, ref UnitBehaviourComponent ub)
                                {
                                    if (u.player != player)
                                        return;
                                    ub.wanderArea = attackArea;
                                });
                            
                            Entities
                                .WithAll<Unit, DisableAttackComponent>()
                                .ForEach(delegate(Entity ue, ref Unit u)
                                {
                                    if (u.player != player)
                                        return;
                                    PostUpdateCommands.RemoveComponent<DisableAttackComponent>(ue);
                                });
                            break;
                        case PlayerBehaviour.defensive:
                            // could be processed all the time, not only here...
                            
                            Entities.WithAll<Unit, UnitBehaviourComponent>()
                                .ForEach(delegate(Entity unitEntity, ref Unit u, ref UnitBehaviourComponent ub)
                                {
                                    if (u.player != player)
                                        return;
                                    ub.wanderArea = defendArea;
                                });
                            
                            // While we are on defensive mode, we use the defend center as the center of chase target
                            Entities
                                .WithAll<Unit, AttackComponent>()
                                .ForEach(delegate(Entity unitEntity, ref Unit u, ref AttackComponent attack)
                                {
                                    if (u.player != player)
                                        return;
                                    attack.chaseCenter = defendCenter;
                                });
                            
                            Entities
                                .WithNone<DisableAttackComponent>()
                                .WithAll<Unit, Translation>()
                                .ForEach(delegate(Entity ue, ref Unit u, ref Translation ut)
                                {
                                    if (u.player != player)
                                        return;

                                    if (math.distancesq(ut.Value, defendCenter) > defendRange)
                                    {
                                        PostUpdateCommands.AddComponent<DisableAttackComponent>(ue);
                                    }
                                });
                            
                            Entities
                                .WithAll<DisableAttackComponent, Unit, Translation>()
                                .ForEach(delegate(Entity ue, ref Unit u, ref Translation ut)
                                {
                                    if (u.player != player)
                                        return;

                                    if (math.distancesq(ut.Value, defendCenter) < defendRange)
                                    {
                                        PostUpdateCommands.RemoveComponent<DisableAttackComponent>(ue);
                                    }
                                });
                            
                            Entities
                                .WithAll<ChaseTargetComponent, MovementAction, DisableAttackComponent>()
                                .ForEach(delegate(Entity e)
                                {
                                    PostUpdateCommands.RemoveComponent<MovementAction>(e);
                                });
                            
                            Entities
                                .WithAll<ChaseTargetComponent, DisableAttackComponent>()
                                .ForEach(delegate(Entity e)
                                {
                                    PostUpdateCommands.RemoveComponent<ChaseTargetComponent>(e);
                                });

                            Entities
                                .WithAll<AttackTargetComponent, DisableAttackComponent>()
                                .ForEach(delegate(Entity e)
                                {
                                    PostUpdateCommands.RemoveComponent<AttackTargetComponent>(e);
                                });
                            
                            break;
                    }
                });
            
            // Enable attack again for units in defend area...
        }
    }
}