using System;

namespace HighlightGenerator
{
    public class Message
    {
        public Message(DateTime timeStamp, string username, string content)
        {
            Timestamp = timeStamp;
            Username = username;
            Content = content;
        }

        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
    }
}
