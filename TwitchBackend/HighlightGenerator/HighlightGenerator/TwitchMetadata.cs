using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HighlightGenerator
{
    class TwitchMetadata
    {
        private const string TWITCH_CLIENT_ID = "37v97169hnj8kaoq8fs3hzz8v6jezdj";
        private const string TWITCH_CLIENT_ID_HEADER = "Client-ID";
        private const string TWITCH_V5_ACCEPT = "application/vnd.twitchtv.v5+json";
        private const string TWITCH_V5_ACCEPT_HEADER = "Accept";
        private const string TWITCH_AUTHORIZATION_HEADER = "Authorization";
        WebClient CreateTwitchWebClient()
        {
            WebClient wc = new WebClient();
            wc.Headers.Add(TWITCH_CLIENT_ID_HEADER, TWITCH_CLIENT_ID);
            wc.Headers.Add(TWITCH_V5_ACCEPT_HEADER, TWITCH_V5_ACCEPT);
            wc.Encoding = Encoding.UTF8;
            return wc;
        }


        public TwitchVideo GetTwitchVideoFromId(int id)
        {
            using (WebClient webClient = CreateTwitchWebClient())
            {
                try
                {
                    string result = webClient.DownloadString(string.Format("https://api.twitch.tv/kraken/videos/{0}", id));

                    JObject videoJson = JObject.Parse(result);

                    if (videoJson != null)
                    {
                        return ParseVideo(videoJson);
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse resp && resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return null;
        }

        public TwitchVideo ParseVideo(JObject videoJson)
        {
            string channel = videoJson.Value<JObject>("channel").Value<string>("display_name");
            string title = videoJson.Value<string>("title");
            string id = videoJson.Value<string>("_id");
            string game = videoJson.Value<string>("game");
            int views = videoJson.Value<int>("views");
            TimeSpan length = new TimeSpan(0, 0, videoJson.Value<int>("length"));
            Uri url = new Uri(videoJson.Value<string>("url"));
            Uri thumbnail = new Uri(videoJson.Value<JObject>("preview").Value<string>("large"));

            string dateStr = videoJson.Value<string>("published_at");

            if (string.IsNullOrWhiteSpace(dateStr))
            {
                dateStr = videoJson.Value<string>("created_at");
            }

            DateTime recordedDate = DateTime.Parse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            if (id.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                id = id.Substring(1);
            }

            return new TwitchVideo(channel, title, id, game, views, length, recordedDate, thumbnail, url);
        }
    }
}
