namespace HighlightGenerator
{
    /// <summary>
    /// Represents a period of time in a Broadcast that corresponds to a filtered segment of a match.
    /// Stores the chat-log messages sent during a MatchSegment.
    /// </summary>
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

        // Used to ensure that each segment is populated with its representative chat-log messages.
        public bool IsPopulated = false;


    }
}
