using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class BroadcastDownloader
    {
        //TODO check LocalBroadcastManager for locally saved broadcast before committing to download.

        private BroadcastInfo Info;
        private string Channel = "Riotgames";

        public BroadcastDownloader(string url)
        {
            this.Info = GetBroadcastInfo(url);
            
        }

        public BroadcastDownloader(DateTime date)
        {
            this.Info = GetBroadcastInfo(date);
        }

        public BroadcastInfo GetBroadcastInfo(string url)
        {
            return null;
        }

        public BroadcastInfo GetBroadcastInfo(DateTime date)
        {
            return null;
        }

        public Broadcast DownloadBroadcast(BroadcastInfo info)
        {
            return null;
        }
    }
}
