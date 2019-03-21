using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    internal class LocalBroadcastManager
    {
        private string BroadcastPath = ConfigurationManager.AppSettings["RootPath"] + "Broadcasts\\";
        private string BroadcastJson = "Broadcasts.json";

        // Loads in the broadcast list file
        internal LocalBroadcastManager()
        {
            var broadcastJson = File.ReadAllText(BroadcastPath + BroadcastJson);
            this.Broadcasts = JsonConvert.DeserializeObject<List<Broadcast>>(broadcastJson);
        }

        internal LocalBroadcastManager(List<Broadcast> broadcasts)
        {
            this.Broadcasts = broadcasts;
        }

        private List<Broadcast> Broadcasts { get; set; }

        /// <summary>
        /// Adds broadcast to the broadcasts list and creates a JSON copy for offline use.
        /// </summary>
        /// <param name="broadcast"></param>
        internal void AddBroadcast(Broadcast broadcast)
        {
            Console.Out.WriteLine(JsonConvert.SerializeObject(broadcast));
            this.Broadcasts.Add(broadcast);
            File.WriteAllText(BroadcastPath + BroadcastJson, JsonConvert.SerializeObject(Broadcasts));
        }
    }
}
