using System;
using UnityEngine;
using Network.DTO;
using Network.Messages;

namespace Network.Serialization
{
    public static class NetworkSerializer
    {
        public static string SerializeSnapshot(
            SessionSnapshotDTO snapshot)
        {
            if (snapshot == null)
                return null;

            return JsonUtility.ToJson(snapshot);
        }
        
        public static SessionSnapshotDTO DeserializeSnapshot(
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonUtility.FromJson<SessionSnapshotDTO>(json);
        }
        
        public static string SerializeMessage(object message)
        {
            if (message == null)
                return null;

            return JsonUtility.ToJson(message);
        }
        
        public static NetworkMessageType GetMessageType(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON da mensagem está vazio.");

            NetworkMessageHeader header =
                JsonUtility.FromJson<NetworkMessageHeader>(json);

            return header.Type;
        }
        
        public static T DeserializeMessage<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonUtility.FromJson<T>(json);
        }
    }
}