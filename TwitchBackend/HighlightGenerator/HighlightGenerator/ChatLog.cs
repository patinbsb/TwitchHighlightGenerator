using System.Collections.Generic;

namespace HighlightGenerator
{
    public class ChatLog
    {
        public ChatLog(List<Message> messages)
        {
            Messages = messages;
        }

        public List<Message> Messages { get; set; }
    }
}
