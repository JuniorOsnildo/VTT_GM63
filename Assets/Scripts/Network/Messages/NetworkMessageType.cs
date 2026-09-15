namespace Network.Messages
{
    public enum NetworkMessageType
    {
        JoinRequest,
        JoinAccepted,

        SessionSnapshot,

        MoveTokenRequest,
        TokenMoved,

        ActiveRoomChanged
    }
}