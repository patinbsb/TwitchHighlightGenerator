using System;
using System.Collections.Generic;

namespace HighlightGenerator
{
    public class Match
    {
        public Match(DateTime startTime, int id, int broadcastId, bool isInstantReplay, List<MatchSegment> segments)
        {
            StartTime = startTime;
            Id = id;
            BroadcastId = broadcastId;
            IsInstantReplay = isInstantReplay;
            Segments = segments;
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
