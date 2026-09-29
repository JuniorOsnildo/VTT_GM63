using Network.DTO;

namespace Network.Client
{
    public class PlayerSessionState
    {
        public SessionSnapshotDTO Snapshot { get; private set; }

        public void ApplySnapshot(SessionSnapshotDTO snapshot)
        {
            Snapshot = snapshot;
        }

        public void ApplyActiveRoom(RoomDTO room)
        {
            if (Snapshot == null || room == null)
                return;

            Snapshot.ActiveRoomId = room.Id;
            Snapshot.ActiveRoom = room;
        }

        public bool CanPlayerControlToken(string playerId, string tokenId)
        {
            if (Snapshot == null || Snapshot.Players == null)
                return false;

            foreach (PlayerDTO player in Snapshot.Players)
            {
                if (player.Id != playerId)
                    continue;

                if (player.ControlledTokenIds == null)
                    return false;

                foreach (string controlledTokenId in player.ControlledTokenIds)
                {
                    if (controlledTokenId == tokenId)
                        return true;
                }

                return false;
            }

            return false;
        }
        
        public void ApplyPlayerControlUpdated(string playerId, string[] controlledTokenIds)
        {
            if (Snapshot == null || Snapshot.Players == null)
                return;

            foreach (PlayerDTO player in Snapshot.Players)
            {
                if (player.Id != playerId)
                    continue;

                player.ControlledTokenIds = controlledTokenIds;
                return;
            }
        }
    }
}