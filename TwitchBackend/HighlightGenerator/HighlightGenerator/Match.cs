using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class Match
    {
        public Match(DateTime startTime, int id, bool isInstantReplay, ChatLog chatLog)
        {
            this.StartTime = startTime;
            this.Id = id;
            this.IsInstantReplay = isInstantReplay;
            this.ChatLog = chatLog;
        }

        public ChatLog ChatLog { get; set; }
        public bool IsInstantReplay { get; set; }
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
    }
}
