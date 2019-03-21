using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class BroadcastFilter
    {
        public BroadcastFilter(double filterThreshold, int startingFrame, int framesToSkip,
            bool convertToGreyscale, int secondsUntilTimeout, int secondsMinimumMatchLength, Broadcast broadcast)
        {
            this.FilterThreshold = filterThreshold;
            this.StartingFrame = startingFrame;
            this.FramesToSkip = framesToSkip;
            this.ConvertToGreyscale = convertToGreyscale;
            this.SecondsUntilTimeout = secondsUntilTimeout;
            this.SecondsMinimumMatchLength = secondsMinimumMatchLength;
            this.Broadcast = broadcast;
        }

        public Broadcast Broadcast { get; set; }
        public int SecondsMinimumMatchLength { get; set; }
        public int SecondsUntilTimeout { get; set; }
        public bool ConvertToGreyscale { get; set; }
        public int FramesToSkip { get; set; }
        public int StartingFrame { get; set; }
        public double FilterThreshold { get; set; }

        public FilteredMatches FilterBroadcast()
        {
            // Run root/Scripts/...
            // Get results.
            return null;
        }
    }
}
