using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class Match
    {
        public Match(DateTime startTime, int id, int broadcastId, bool isInstantReplay, List<MatchSegment> segments)
        {
            this.StartTime = startTime;
            this.Id = id;
            this.BroadcastId = broadcastId;
            this.IsInstantReplay = isInstantReplay;
            this.Segments = segments;
        }
        public bool IsInstantReplay { get; set; }
        public int Id { get; set; }
        public int BroadcastId { get; set; }
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

        public string GetFileName(bool withExtension = true)
        {
            if (IsInstantReplay)
            {
                if (withExtension)
                {
                    return "highlight" + Id + ".mp4";
                }
                else
                {
                    return "highlight" + Id;
                }
            }
            else
            {
                if (withExtension)
                {
                    return "match" + Id + ".mp4";
                }
                else
                {
                    return "match" + Id;
                }
            }
        }
    }
}
