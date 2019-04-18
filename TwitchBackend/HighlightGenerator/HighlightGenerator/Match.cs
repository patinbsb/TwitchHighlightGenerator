using System;
using System.Collections.Generic;

namespace HighlightGenerator
{
    /// <summary>
    /// Represents the slices of a Broadcast that describes either a full match or an instant replay.
    /// </summary>
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
        // If a match is of a small enough length it is considered an instant replay. Used for training the Neural net.
        public bool IsInstantReplay { get; set; }
        // Match Id.
        public int Id { get; set; }
        // Links to the Broadcast this match was rendered from.
        public int BroadcastId { get; set; }
        // The start time of the match.
        public DateTime StartTime { get; set; }
        // As matches are rarely a continuous slice of video, they are described in a series of start and end times called segments.
        public List<MatchSegment> Segments { get; set; }
        // Have the matches corresponding chat-logs been loaded into the memory of this object?
        public bool IsPopulated = false;

        /// <summary>
        /// Routine to load a slice of chat-logs from our local database that represent the messages sent during this match.
        /// </summary>
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

        /// <summary>
        /// Useful for getting the name of the physical file this match represents.
        /// Instant replays are named highlights instead of matches.
        /// </summary>
        /// <param name="withExtension"></param>
        /// <returns></returns>
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
