using System.Collections.Generic;

namespace HighlightGenerator
{
    /// <summary>
    /// Maintains a reference to a collection of Twitch chat messages.
    /// </summary>
    public class ChatLog
    {
        public ChatLog(List<Message> messages)
        {
            Messages = messages;
        }

        public List<Message> Messages { get; set; }
    }
}
