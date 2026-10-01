using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.UI
{
    public class RoomListItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomNameText;
        [SerializeField] private Button openButton;
        [SerializeField] private Button deleteButton;
        
        [SerializeField] private Color normalNameColor = Color.white;
        [SerializeField] private Color activeNameColor = Color.green;

        private Action<string> onOpen;
        private Action<string> onDelete;
        
        private string roomId;

        public void Setup(string id, string roomName, bool isActive, Action<string> openAction, Action<string> deleteAction)
        {
            roomId = id;
            roomNameText.text = roomName;
            roomNameText.color = isActive ? activeNameColor : normalNameColor;

            onOpen = openAction;
            onDelete = deleteAction;

            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(() => onOpen?.Invoke(roomId));

            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => onDelete?.Invoke(roomId));
        }
    }
}