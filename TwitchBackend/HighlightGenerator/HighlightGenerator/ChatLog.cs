using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    public class ChatLog
    {
        public ChatLog(List<Message> messages)
        {
            this.Messages = messages;
        }

        public List<Message> Messages { get; set; }
    }
}
