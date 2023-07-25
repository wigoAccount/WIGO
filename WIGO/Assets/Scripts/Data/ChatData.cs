using System;
using System.Collections.Generic;
using UnityEngine;
using WIGO.Userinterface;

namespace WIGO.Core
{
    [Serializable]
    public class ChatData
    {
        [SerializeField] string userid;
        [SerializeField] int mutestatus;
        [SerializeField] int chatcategory;
        [SerializeField] List<ChatMessage> _content;

        public ChatData(ChatCategory category, IEnumerable<ChatMessage> content, bool muted = false)
        {
            mutestatus = muted ? 1 : 0;
            chatcategory = (int)category;
            _content = new List<ChatMessage>(content);
        }

        public string GetUserId() => userid;
        public bool IsMuted() => mutestatus > 0;
        public ChatCategory GetChatCategory() => (ChatCategory)chatcategory;
        public IEnumerable<ChatMessage> GetMessages() => _content;
        public ChatMessage GetLastMessage() => _content.Count > 0 ? _content[0] : null;

        public void ChangeMuteStatus(bool muted) => mutestatus = muted ? 1 : 0;
        public void AddMessage(ChatMessage message)
        {
            _content.Insert(0, message);
        }

        public static ChatData CreateRandom(TempMessagesContainer container)
        {
            var muted = UnityEngine.Random.Range(0f, 1f) > 0.8f;
            int rnd = UnityEngine.Random.Range(1, 3);
            var category = (ChatCategory)rnd;

            int msgCount = UnityEngine.Random.Range(5, 400);
            List<ChatMessage> content = new List<ChatMessage>();
            for (int i = 0; i < msgCount; i++)
            {
                string text = container.GetRandomMessage();
                ChatMessage msg = ChatMessage.CreateRandom(text);
                content.Add(msg);
            }

            return new ChatData(category, content, muted);
        }
    }

    [Serializable]
    public class ChatMessage
    {
        [SerializeField] string message;
        [SerializeField] int msgtype;
        [SerializeField] string time;
        [SerializeField] int status;

        public ChatMessage(string msg, DateTime sendTime, bool mine = true, int sendStatus = 0)
        {
            message = msg;
            msgtype = mine ? 1 : 0;
            time = sendTime.ToString("HH:mm");
            status = sendStatus;
        }

        public string GetText() => message;
        public bool IsMine() => msgtype > 0;
        public string GetSendTime() => time;
        public int GetStatus() => status;

        public static ChatMessage CreateRandom(string message)
        {
            bool isMine = UnityEngine.Random.Range(0f, 1f) > 0.5f;
            DateTime sendTime = SetRandomTime();
            //string msg = isMine ? $"test message MY #{index}" : $"test message HIS #{index}";
            int sendStatus = UnityEngine.Random.Range(0, 3);

            return new ChatMessage(message, sendTime, isMine, sendStatus);
        }

        static DateTime SetRandomTime()
        {
            System.Random random = new System.Random();
            TimeSpan start = TimeSpan.FromHours(1);
            TimeSpan end = TimeSpan.FromHours(23);
            int maxMinutes = (int)((end - start).TotalMinutes);

            int minutes = random.Next(maxMinutes);
            TimeSpan time = start.Add(TimeSpan.FromMinutes(minutes));
            DateTime sendTime = DateTime.Now + time;
            return sendTime;//string.Format(@"{0:hh\:mm}", time);
        }
    }
}
