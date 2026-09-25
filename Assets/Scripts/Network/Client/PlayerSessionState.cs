using Network.DTO;

namespace Network.Client
{
    public class PlayerSessionState
    {
        public SessionSnapshotDTO Snapshot { get; private set; }

        public void ApplySnapshot(
            SessionSnapshotDTO snapshot)
        {
            Snapshot = snapshot;
        }
    }
}