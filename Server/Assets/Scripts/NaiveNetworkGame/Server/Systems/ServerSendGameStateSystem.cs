using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;

namespace NaiveNetworkGame.Server.Systems
{
    public class ServerSendGameStateSystem : ComponentSystem
    {
        private float sendGameStateTime;
        private float sendTranslationStateTime;

        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<ServerSingleton>();
        }

        protected override void OnUpdate()
        {
            var serverEntity = GetSingletonEntity<ServerSingleton>();
            var server =
                EntityManager.GetSharedComponentData<ServerSingleton>(serverEntity);
            
            var networkManager = server.networkManager;

            if (networkManager == null)
                return;
            
            ServerNetworkStatistics.outputBytesLastFrame = 0;
            ServerNetworkStatistics.currentConnections = 0;

            sendGameStateTime += Time.DeltaTime;
            sendTranslationStateTime += Time.DeltaTime;
            
            // First, for each connection, send player id
            var m_Driver = networkManager.m_Driver;
            
            Entities
                .WithNone<PlayerConnectionSynchronized>()
                .WithAll<PlayerController, PlayerConnectionId>()
                .ForEach(delegate(Entity pe, ref PlayerConnectionId p, ref PlayerController playerController)
                {
                    var writer = m_Driver.BeginSend(p.connection);
                    writer.WriteByte(PacketType.ServerSendPlayerId);
                    writer.WriteByte(playerController.player);
                    m_Driver.EndSend(writer);

                    ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                    ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;

                    PostUpdateCommands.AddComponent<PlayerConnectionSynchronized>(pe);
                });

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
                    Entities
                        .WithAll<NetworkPlayerState>()
                        .ForEach(delegate(ref NetworkPlayerState n, ref PlayerConnectionId p)
                        {
                            // only send player state to each player...
                            if (p.connection == connection)
                            {
                                var writer = m_Driver.BeginSend(connection);
                                n.Write(ref writer);
                                m_Driver.EndSend(writer);

                                ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                                ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                            }
                        });

                    var count = Entities
                        .WithAll<NetworkGameState>().ToEntityQuery().CalculateEntityCount();
                    
                    var writer = m_Driver.BeginSend(server.framentationPipeline, connection, 
                        sizeof(byte) + sizeof(ushort) +
                        NetworkGameState.GetSize() * count);
                    
                    writer.WriteByte(PacketType.ServerGameState);
                    writer.WriteUShort((ushort) count);

                    Entities
                        .WithAll<NetworkGameState>()
                        .ForEach(delegate(ref NetworkGameState n)
                        {
                            n.Write(ref writer);
                        });
                    
                    m_Driver.EndSend(writer);
                    
                    ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                    ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                }

                if (sendTranslation)
                {
                    var count = Entities
                        .WithAll<NetworkTranslationSync>().ToEntityQuery().CalculateEntityCount();
                    
                    var writer = m_Driver.BeginSend(server.framentationPipeline, connection, 
                        sizeof(byte) + sizeof(ushort) +
                        NetworkTranslationSync.GetSize() * count);
                    
                    writer.WriteByte(PacketType.ServerTranslationSync);
                    writer.WriteUShort((ushort) count);
                    
                    Entities
                        .WithAll<NetworkTranslationSync>()
                        .ForEach(delegate(ref NetworkTranslationSync n)
                        {
                            n.Write(ref writer);
                        });
                    
                    m_Driver.EndSend(writer);
                    
                    ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                    ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                }
            }
        }
    }
}