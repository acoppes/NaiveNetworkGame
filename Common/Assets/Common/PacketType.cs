namespace NaiveNetworkGame.Common
{
    public static class PacketType
    {
        public static readonly byte ServerSendPlayerId = 1;
        public static readonly byte ServerGameState = 2;
        public static readonly byte ClientKeepAlive = 50 + 1;
        public static readonly byte ClientPlayerAction = 50 + 2;
    }
}