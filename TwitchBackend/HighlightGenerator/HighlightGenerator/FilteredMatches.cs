using System;
using System.Collections.Generic;

namespace HighlightGenerator
{
    public class FilteredMatches
    {
        public FilteredMatches(List<Match> matches, Broadcast broadcast)
        {
            Matches = matches;
            Broadcast = broadcast;
        }

        public List<Match> Matches { get; set; }
        public Broadcast Broadcast { get; set; }
        public bool IsPopulated = false;

        public void PopulateMatchChatLogs()
        {
            foreach (var match in Matches)
            {
                Console.WriteLine($"Populating broadcast {Broadcast.Id}, match {match.Id}");
                if (!match.IsPopulated)
                {
                    match.PopulateSegmentChatLogs();
                }
            }
            IsPopulated = true;
        }

        public string GetDirectoryPath()
        {
            return Helper.BroadcastsPath + Broadcast.Id + "\\";
        }
    }
}
