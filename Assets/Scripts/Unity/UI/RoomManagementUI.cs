using Session;
using Unity.UI;
using UnityEngine;

namespace VTT.Unity
{
    public class RoomManagementUI : MonoBehaviour
    {
        [SerializeField] private SessionManager sessionManager;
        [SerializeField] private Transform roomListContent;
        [SerializeField] private RoomListItemUI roomListItemPrefab;
        
        private void Start()
        {
            sessionManager.OnRoomCreated += HandleRoomChanged;
            sessionManager.OnRoomDeleted += HandleRoomChanged;

            RefreshRoomList();
        }

        private void HandleRoomChanged(Room room)
        {
            RefreshRoomList();
        }

        private void OnDestroy()
        {
            if (sessionManager == null)
                return;

            sessionManager.OnRoomCreated -= HandleRoomChanged;
            sessionManager.OnRoomDeleted -= HandleRoomChanged;
        }

        private void RefreshRoomList()
        {
            if (sessionManager == null)
                return;

            GameSession gameSession = sessionManager.GetGameSession();

            if (gameSession == null)
                return;

            foreach (Transform child in roomListContent)
            {
                Destroy(child.gameObject);
            }

            foreach (Room room in gameSession.Rooms)
            {
                RoomListItemUI item = Instantiate(roomListItemPrefab, roomListContent);

                item.Setup(room.Id, room.Name, OpenRoom);
            }
        }
        
        private void OpenRoom(string roomId)
        {
            GameSession gameSession = sessionManager.GetGameSession();

            if (gameSession == null)
                return;

            Room room = gameSession.GetRoomById(roomId);

            if (room == null)
                return;

            sessionManager.SwitchRoom(room);
        }
    }
}