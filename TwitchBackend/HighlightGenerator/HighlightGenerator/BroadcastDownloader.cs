using System;

namespace HighlightGenerator
{
    public class BroadcastDownloader
    {
        //TODO check LocalBroadcastManager for locally saved broadcast before committing to download.

        private Broadcast _info;
        private string _channel = "Riotgames";

        public BroadcastDownloader(string url)
        {
            _info = GetBroadcastInfo(url);

        }

        public BroadcastDownloader(DateTime date)
        {
            _info = GetBroadcastInfo(date);
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
