namespace NaiveNetworkGame.Common
{
    public static class PacketType
    {
        public static readonly byte ServerSendPlayerId = 1;
        public static readonly byte ServerGameState = 2;
        public static readonly byte ServerPlayerState = 3;
        public static readonly byte ServerTranslationSync = 4;
        
        public static readonly byte ServerEmptyGameState = 5;

        public static readonly byte ServerDeniedConnectionMaxPlayers = 30;
        public static readonly byte ServerSimulationStarted = 31;        
        
        public static readonly byte ClientKeepAlive = 50 + 1;
        public static readonly byte ClientPlayerAction = 50 + 2;
        public static readonly byte ClientDisconnect = 50 + 3;

    }
}