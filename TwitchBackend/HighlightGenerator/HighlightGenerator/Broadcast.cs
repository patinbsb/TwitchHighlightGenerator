using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    public class Broadcast
    {
        public Broadcast(ChatLog chatLog, BroadcastInfo broadcastInfo)
        {
            this.ChatLog = chatLog;
            this.BroadcastInfo = broadcastInfo;
        }

        public ChatLog ChatLog { get; set; }
        public BroadcastInfo BroadcastInfo { get; set; }

        public string GetFilePath()
        {
            return ConfigurationManager.AppSettings["RootPath"] + "Broadcasts\\" + BroadcastInfo.Id.ToString();
        }
    }
}
