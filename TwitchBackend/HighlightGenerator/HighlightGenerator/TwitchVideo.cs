using System;

namespace HighlightGenerator
{
    public class TwitchVideo
    {
        #region Constatnts

        private const string UntitledBroadcast = "Untitled Broadcast";
        private const string UnknownGame = "Unknown";

        #endregion Constatnts

        #region Constructors

        public TwitchVideo(string channel, string title, string id, string game, int views, TimeSpan length,
            DateTime recordedDate, Uri thumbnail, Uri url)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = UntitledBroadcast;
            }

            Channel = channel;
            Title = title;
            Id = id;

            Game = string.IsNullOrWhiteSpace(game) ? UnknownGame : game;

            Views = views;
            Length = length;
            RecordedDate = recordedDate;
            Thumbnail = thumbnail ?? throw new ArgumentNullException(nameof(thumbnail));
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        #endregion Constructors

        #region Properties

        public string Channel { get; }

        public string Title { get; }

        public string Id { get; }

        public string Game { get; }

        public TimeSpan Length { get; }

        public int Views { get; }

        public DateTime RecordedDate { get; }

        public Uri Thumbnail { get; }

        public Uri Url { get; }

        #endregion Properties
    }
}
