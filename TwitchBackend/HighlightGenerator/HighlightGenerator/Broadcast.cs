using System;
using System.Configuration;

namespace HighlightGenerator
{
    public class Broadcast
    {
        public Broadcast(int id, DateTime startTime)
        {
            Id = id;
            StartTime = startTime;
        }
        public int Id { get; set; }
        public DateTime StartTime { get; set; }

        public string GetFilePath()
        {
            return ConfigurationManager.AppSettings["RootPath"] + "Broadcasts\\" + Id.ToString();
        }
    }
}
