using System;
using System.IO;
using Network.DTO;
using Session;
using UnityEngine;

namespace VTT.Unity
{
    public class SaveManager : MonoBehaviour
    {
        private string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
        private string SavePath => Path.Combine(SaveDirectory, "campaign.json");

        [Serializable]
        public class SaveData
        {
            public string ActiveRoomId;
            public RoomDTO[] Rooms;
        }

        public void Save(GameSession gameSession)
        {
            if (gameSession == null)
                return;

            SaveData saveData = new SaveData
            {
                ActiveRoomId = gameSession.ActiveRoom?.Id,
                Rooms = new RoomDTO[gameSession.Rooms.Count]
            };

            for (int i = 0; i < gameSession.Rooms.Count; i++)
            {
                saveData.Rooms[i] = Network.DTO.DTOConverter.ToDTO(gameSession.Rooms[i]);
            }

            Directory.CreateDirectory(SaveDirectory);

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);

            Debug.Log($"[SAVE] Sessão salva em: {SavePath}");
        }
        
        public SaveData Load()
        {
            if (!File.Exists(SavePath))
                return null;

            string json = File.ReadAllText(SavePath);

            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
                return null;

            Debug.Log($"[SAVE] Sessão carregada de: {SavePath}");

            return saveData;
        }
        
        public void LoadIntoSession(SessionManager sessionManager)
        {
            SaveData saveData = Load();

            if (saveData == null)
                return;

            sessionManager.LoadFromSave(saveData);
        }
    }
}