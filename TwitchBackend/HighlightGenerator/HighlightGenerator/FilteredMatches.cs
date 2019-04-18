using System;
using System.Collections.Generic;

namespace HighlightGenerator
{
    /// <summary>
    /// Responsible for representing the filtered collection of matches that were generated from their corresponding broadcast.
    /// </summary>
    public class FilteredMatches
    {
        public FilteredMatches(List<Match> matches, Broadcast broadcast)
        {
            Matches = matches;
            Broadcast = broadcast;
        }

        public List<Match> Matches { get; set; }
        public Broadcast Broadcast { get; set; }

        // Marks if all matches in this object have had their chat-logs populated from the local database yet.
        public bool IsPopulated = false;

        /// <summary>
        /// Loads the relevant per match Twitch chat-logs from the local database into memory.
        /// </summary>
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

        /// <summary>
        /// Useful for finding the location of this objects corresponding broadcast location.
        /// </summary>
        /// <returns></returns>
        public string GetDirectoryPath()
        {
            return Helper.BroadcastsPath + Broadcast.Id + "\\";
        }
    }
}
