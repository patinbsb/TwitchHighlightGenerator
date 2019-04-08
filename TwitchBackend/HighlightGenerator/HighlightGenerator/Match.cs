using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class Match
    {
        public Match(DateTime startTime, int id, bool isInstantReplay, List<MatchSegment> segments)
        {
            this.StartTime = startTime;
            this.Id = id;
            this.IsInstantReplay = isInstantReplay;
            this.Segments = segments;
        }
        public bool IsInstantReplay { get; set; }
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public List<MatchSegment> Segments { get; set; }
        public bool IsPopulated = false;

        public void PopulateSegmentChatLogs()
        {
            foreach (var segment in Segments)
            {
                if (!segment.IsPopulated)
                {
                    segment.ChatLog = ChatLogParser.GetChatInRange(StartTime, segment.StartTime, segment.EndTime);
                    segment.IsPopulated = true;
                }
            }
            IsPopulated = true;
        }
    }
}
