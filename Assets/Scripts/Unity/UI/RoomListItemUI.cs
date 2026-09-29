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

        private Action<string> onOpen;
        
        private string roomId;

        public void Setup(string id, string roomName, Action<string> openAction)
        {
            roomId = id;
            roomNameText.text = roomName;
            onOpen = openAction;

            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(() => onOpen?.Invoke(roomId));
        }
    }
}