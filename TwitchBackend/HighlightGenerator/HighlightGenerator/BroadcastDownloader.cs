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

        private Broadcast Info;
        private string Channel = "Riotgames";

        public BroadcastDownloader(string url)
        {
            this.Info = GetBroadcastInfo(url);

        }

        public BroadcastDownloader(DateTime date)
        {
            this.Info = GetBroadcastInfo(date);
        }

        public Broadcast GetBroadcastInfo(string url)
        {
            return null;
        }

        public Broadcast GetBroadcastInfo(DateTime date)
        {
            return null;
        }

        public Broadcast DownloadBroadcast(Broadcast info)
        {
            return null;
        }
    }
}
