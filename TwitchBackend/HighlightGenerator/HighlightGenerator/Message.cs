using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    public class Message
    {
        public Message(DateTime timeStamp, string username, string content)
        {
            this.Timestamp = timeStamp;
            this.Username = username;
            this.Content = content;
        }

        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
    }
}
