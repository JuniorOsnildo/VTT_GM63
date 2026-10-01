using System;
using Network.DTO;

namespace Network.Messages
{
    [Serializable]
    public class SessionSnapshotMessage
    {
        public NetworkMessageType Type;

        public SessionSnapshotDTO Snapshot;

        public SessionSnapshotMessage(
            SessionSnapshotDTO snapshot)
        {
            Type = NetworkMessageType.SessionSnapshot;
            Snapshot = snapshot;
        }
    }
}