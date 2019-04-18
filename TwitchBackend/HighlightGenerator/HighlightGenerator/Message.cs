using System;

namespace HighlightGenerator
{
    /// <summary>
    /// The atomic unit of a chat-log.
    /// Represents a twitch chat user sending a message at a particular time.
    /// </summary>
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
