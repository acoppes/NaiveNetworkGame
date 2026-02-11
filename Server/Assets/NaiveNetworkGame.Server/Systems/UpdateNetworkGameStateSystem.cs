using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct UpdateNetworkGameStateSystem : ISystem
    {
        private int frame;

        public void OnCreate(ref SystemState state)
        {
            frame = 0;
            // TODO: store frame in an entity + singleton component
        }

        public void OnUpdate(ref SystemState state)
        {
            frame++;
            
            // var delta = SystemAPI.Time.DeltaTime;
            
            // foreach (var networkGameState in 
            //     SystemAPI.Query<RefRW<NetworkGameState>>())
            // {
            //     networkGameState.ValueRW.frame = frame;
            //     // n.delta = delta;
            //     // n.delta = ServerNetworkStaticData.sendGameStateFrequency;
            //     // n.syncVersion = n.version;
            // }
            
            foreach (var (unit, networkGameState) in 
                SystemAPI.Query<RefRO<Unit>, RefRW<NetworkGameState>>()
                    .WithAll<ServerOnly>())
            {
                ref var n = ref networkGameState.ValueRW;
                
                n.unitId = unit.ValueRO.id;
                n.playerId = unit.ValueRO.player;
                n.unitType = unit.ValueRO.type;
                // n.unitType = u.player % 2;
            }
            
            foreach (var (unit, localTransform, networkTranslationSync) in 
                SystemAPI.Query<RefRO<Unit>, RefRO<LocalTransform>, RefRW<NetworkTranslationSync>>()
                    .WithAll<ServerOnly>())
            {
                networkTranslationSync.ValueRW.unitId = unit.ValueRO.id;
                networkTranslationSync.ValueRW.translation = localTransform.ValueRO.Position.xy;
                networkTranslationSync.ValueRW.delta = ServerNetworkStaticData.sendTranslationStateFrequency;
            }
            
            foreach (var (lookingDirection, networkGameState) in 
                SystemAPI.Query<RefRO<LookingDirection>, RefRW<NetworkGameState>>()
                    .WithAll<ServerOnly>())
            {
                networkGameState.ValueRW.lookingDirectionAngleInDegrees = (ushort) 
                    Mathf.RoundToInt(Vector2.Angle(Vector2.right, lookingDirection.ValueRO.direction));
                // n.lookingDirection = l.direction;
            }
            
            foreach (var (unitState, networkGameState) in 
                SystemAPI.Query<RefRO<UnitStateComponent>, RefRW<NetworkGameState>>()
                    .WithAll<ServerOnly>())
            {
                networkGameState.ValueRW.state = unitState.ValueRO.state;
                networkGameState.ValueRW.statePercentage = unitState.ValueRO.percentage;
            }
            
            foreach (var (skin, networkGameState) in 
                SystemAPI.Query<RefRO<Skin>, RefRW<NetworkGameState>>()
                    .WithAll<ServerOnly>())
            {
                networkGameState.ValueRW.skinType = skin.ValueRO.type;
            }
            
            foreach (var (health, networkGameState) in 
                SystemAPI.Query<RefRO<Health>, RefRW<NetworkGameState>>()
                    .WithAll<ServerOnly>())
            {
                networkGameState.ValueRW.health = (byte) Mathf.RoundToInt(100.0f * health.ValueRO.current / health.ValueRO.total);
            }
            
            foreach (var (playerController, networkPlayerState, playerConnectionId, playerBehaviour) in 
                SystemAPI.Query<RefRO<PlayerController>, RefRW<NetworkPlayerState>, RefRO<PlayerConnectionId>, RefRO<PlayerBehaviour>>()
                    .WithAll<ServerOnly>())
            {
                ref var playerState = ref networkPlayerState.ValueRW;
                
                playerState.player = playerController.ValueRO.player;
                playerState.skinType = playerController.ValueRO.skinType;
                playerState.gold = playerController.ValueRO.gold;
                playerState.maxUnits = playerController.ValueRO.maxUnits;
                playerState.currentUnits = playerController.ValueRO.currentUnits;
                playerState.buildingSlots = playerController.ValueRO.availableBuildingSlots;
                playerState.freeBarracks = playerController.ValueRO.freeBarracksCount;
                playerState.behaviourMode = playerBehaviour.ValueRO.mode;
            }
        }
    }
}
