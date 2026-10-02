using Session;
using TMPro;
using Unity.UI;
using UnityEngine;

namespace VTT.Unity
{
    public class RoomManagementUI : MonoBehaviour
    {
        [SerializeField] private RoomListItemUI roomListItemPrefab;
        [SerializeField] private SessionManager sessionManager;
        [SerializeField] private Transform roomListContent;
        
        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private TMP_InputField roomWidthInput;
        [SerializeField] private TMP_InputField roomHeightInput;
        
        [SerializeField] private GameObject deleteConfirmationPanel;
        [SerializeField] private GameObject createRoomPanel;
        
        private string roomToDeleteId;
        
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

                bool isActive = room == gameSession.ActiveRoom;
                item.Setup(room.Id, room.Name, isActive, OpenRoom, OpenDeleteConfirmation);
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
            RefreshRoomList();
        }
        
        public void OpenPanel()
        {
            gameObject.SetActive(true);
        }

        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }
        
        public void CreateRoom()
        {
            if (sessionManager == null)
                return;

            string roomName = roomNameInput.text.Trim();

            if (string.IsNullOrWhiteSpace(roomName))
                return;

            if (!int.TryParse(roomWidthInput.text, out int width))
                return;

            if (!int.TryParse(roomHeightInput.text, out int height))
                return;

            if (width <= 0 || height <= 0)
                return;

            if (width > 100 || height > 100)
                return;
            
            Room room = sessionManager.CreateRoom(roomName, width, height);

            if (room == null)
                return;

            sessionManager.SwitchRoom(room);

            roomNameInput.text = "";
            roomWidthInput.text = "";
            roomHeightInput.text = "";

            ClosePanel();
            CloseCreateRoomPanel();
        }
        
        public void OpenCreateRoomPanel()
        {
            createRoomPanel.SetActive(true);
        }

        public void CloseCreateRoomPanel()
        {
            createRoomPanel.SetActive(false);
        }
        
        public void OpenDeleteConfirmation(string roomId)
        {
            roomToDeleteId = roomId;
            deleteConfirmationPanel.SetActive(true);
        }

        public void CloseDeleteConfirmation()
        {
            roomToDeleteId = null;
            deleteConfirmationPanel.SetActive(false);
        }
        
        public void ConfirmDeleteRoom()
        {
            if (string.IsNullOrEmpty(roomToDeleteId))
                return;

            GameSession gameSession = sessionManager.GetGameSession();

            if (gameSession == null)
                return;

            Room room = gameSession.GetRoomById(roomToDeleteId);

            if (room == null)
            {
                CloseDeleteConfirmation();
                return;
            }

            sessionManager.DeleteRoom(room);
            CloseDeleteConfirmation();
        }
    }
}