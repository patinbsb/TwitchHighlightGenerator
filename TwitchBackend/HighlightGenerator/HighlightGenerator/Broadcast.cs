using System;
using System.Configuration;

namespace HighlightGenerator
{
    /// <summary>
    /// Represents a raw unfiltered broadcast video file.
    /// </summary>
    public class Broadcast
    {
        public Broadcast(int id, DateTime startTime)
        {
            Id = id;
            StartTime = startTime;
        }
        // Unique Twitch identifier for this particular broadcast.
        public int Id { get; set; }
        // The point when the first frame of the video occurs.
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Returns the folder location of the represented video file.
        /// </summary>
        /// <returns></returns>
        public string GetFilePath()
        {
            return ConfigurationManager.AppSettings["RootPath"] + "Broadcasts\\" + Id.ToString();
        }
    }
}
