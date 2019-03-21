using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    public class BroadcastInfo
    {
        public BroadcastInfo(int id, DateTime startTime)
        {
            this.Id = id;
            this.StartTime = startTime;
        }
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
    }
}
