using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial struct ServerSendGameStateSystem : ISystem
    {
        private float sendGameStateTime;
        private float sendTranslationStateTime;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ServerSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var serverEntity = SystemAPI.GetSingletonEntity<ServerSingleton>();
            var server = state.EntityManager.GetSharedComponentManaged<ServerData>(serverEntity);
            
            var networkManager = server.networkManager;

            if (networkManager == null)
                return;
            
            ServerNetworkStatistics.outputBytesLastFrame = 0;
            ServerNetworkStatistics.currentConnections = 0;

            var dt = SystemAPI.Time.DeltaTime;
            
            sendGameStateTime += dt;
            sendTranslationStateTime += dt;
            
            // First, for each connection, send player id
            var m_Driver = networkManager.m_Driver;
            
            // Send simulation started packet to all connections,
            // we are not waiting for players anymore... reliablity pipeline!
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (playerConnectionId, entity) in 
                SystemAPI.Query<RefRW<PlayerConnectionId>>()
                    .WithEntityAccess())
            {
                if (!playerConnectionId.ValueRO.simulationStarted)
                {
                    // var writer = m_Driver.BeginSend(server.reliabilityPipeline, p.connection);
                    m_Driver.BeginSend(server.reliabilityPipeline, playerConnectionId.ValueRO.connection, out var writer);
                    writer.WriteByte(PacketType.ServerSimulationStarted);
                    m_Driver.EndSend(writer);
                }
                playerConnectionId.ValueRW.simulationStarted = true;
            }
            
            foreach (var (playerConnectionId, playerController, playerEntity) in 
                SystemAPI.Query<RefRO<PlayerConnectionId>, RefRO<PlayerController>>()
                    .WithNone<PlayerConnectionSynchronized>()
                    .WithEntityAccess())
            {
                // var writer = m_Driver.BeginSend(server.reliabilityPipeline, p.connection);
                m_Driver.BeginSend(server.reliabilityPipeline, playerConnectionId.ValueRO.connection, out var writer);
                writer.WriteByte(PacketType.ServerSendPlayerId);
                writer.WriteByte(playerController.ValueRO.player);

                var playerActions = state.EntityManager.GetBuffer<PlayerAction>(playerEntity);
                
                writer.WriteByte((byte) playerActions.Length);
                for (var i = 0; i < playerActions.Length; i++)
                {
                    writer.WriteByte(playerActions[i].type);
                    writer.WriteByte(playerActions[i].cost);
                }
                
                m_Driver.EndSend(writer);

                ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;

                ecb.AddComponent<PlayerConnectionSynchronized>(playerEntity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
            var sendTranslation = false;
            var sendOtherState = false;

            if (sendTranslationStateTime > ServerNetworkStaticData.sendTranslationStateFrequency)
            {
                sendTranslationStateTime -= ServerNetworkStaticData.sendTranslationStateFrequency;
                sendTranslation = true;
            }

            if (sendGameStateTime > ServerNetworkStaticData.sendGameStateFrequency)
            {
                sendGameStateTime -= ServerNetworkStaticData.sendGameStateFrequency;
                sendOtherState = true;
            }
            
            for (var i = 0; i < networkManager.m_Connections.Length; i++)
            {
                var connection = networkManager.m_Connections[i];
                
                if (!connection.IsCreated)
                {
                    // should we destroy player controller?
                    // how to send that with gamestate, maybe mark as destroyed...
                    continue;
                }

                if (sendOtherState)
                {
                    foreach (var (networkPlayerState, playerConnectionId) in 
                        SystemAPI.Query<RefRO<NetworkPlayerState>, RefRO<PlayerConnectionId>>())
                    {
                        // only send player state to each player...
                        if (playerConnectionId.ValueRO.connection == connection)
                        {
                            // var writer = m_Driver.BeginSend(connection);
                            m_Driver.BeginSend(connection, out var writer);
                            writer.WriteByte(PacketType.ServerPlayerState);
                            networkPlayerState.ValueRO.Write(ref writer);
                            m_Driver.EndSend(writer);
                        
                            ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                            ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                        }
                    }

                    var gameStateQuery = SystemAPI.QueryBuilder().WithAll<NetworkGameState>().Build();
                    var count = gameStateQuery.CalculateEntityCount();

                    if (count == 0)
                    {
                        // var writer = m_Driver.BeginSend(connection);
                        m_Driver.BeginSend(connection, out var writer);
                        writer.WriteByte(PacketType.ServerEmptyGameState);
                        m_Driver.EndSend(writer);
                        
                        ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                        ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                    }
                    else
                    {
                        // var writer = m_Driver.BeginSend(server.framentationPipeline, connection, 
                        //     sizeof(byte) + sizeof(ushort) +
                        //     NetworkGameState.GetSize() * count);
                        m_Driver.BeginSend(server.fragmentationPipeline, connection, out var writer, sizeof(byte) + sizeof(ushort) +
                            NetworkGameState.GetSize() * count);
                    
                        writer.WriteByte(PacketType.ServerGameState);
                        writer.WriteUShort((ushort) count);

                        foreach (var networkGameState in 
                            SystemAPI.Query<RefRO<NetworkGameState>>())
                        {
                            networkGameState.ValueRO.Write(ref writer);
                        }
                    
                        m_Driver.EndSend(writer);
                        
                        ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                        ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                    }
                }

                if (sendTranslation)
                {
                    var translationSyncQuery = SystemAPI.QueryBuilder().WithAll<NetworkTranslationSync>().Build();
                    var count = translationSyncQuery.CalculateEntityCount();
                    
                    if (count > 0)
                    {
                        // var writer = m_Driver.BeginSend(server.framentationPipeline, connection,
                        //     sizeof(byte) + sizeof(ushort) +
                        //     NetworkTranslationSync.GetSize() * count);
                        
                        m_Driver.BeginSend(server.fragmentationPipeline, connection, out var writer, sizeof(byte) + sizeof(ushort) +
                            NetworkTranslationSync.GetSize() * count);

                        writer.WriteByte(PacketType.ServerTranslationSync);
                        writer.WriteUShort((ushort) count);

                        foreach (var networkTranslationSync in 
                            SystemAPI.Query<RefRO<NetworkTranslationSync>>())
                        {
                            networkTranslationSync.ValueRO.Write(ref writer);
                        }

                        m_Driver.EndSend(writer);

                        ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                        ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                    }
                }
            }
        }
    }
}
