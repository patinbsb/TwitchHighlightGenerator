using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class MatchSegment
    {
        public MatchSegment(double startTime, double endTime, ChatLog chatLog)
        {
            StartTime = startTime;
            EndTime = endTime;
            ChatLog = chatLog;
        }

        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public ChatLog ChatLog { get; set; }
        public bool IsPopulated = false;


    }
}
