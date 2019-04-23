using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using HighlightGenerator;
using HighlightGeneratorTests;

namespace Tests
{
    public class BroadcastFilterTests
    {
        private BroadcastFilter broadcastFilter;
        private List<FilteredMatches> filteredMatches;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestHelper.Initialise();
        }

        [SetUp]
        public void Setup()
        {
            broadcastFilter = new BroadcastFilter();
        }

        [Test]
        public void FilterBroadcastTest()
        {
            // Check for new videos to process.
            var files = Directory.GetFiles(TestHelper.twitchVodsPath);
            List<string> videosToProcess = new List<string>();

            // Filter out non video files.
            foreach (var file in files)
            {
                if (file.EndsWith(".mp4"))
                {
                    videosToProcess.Add(file);
                }
            }

            // Filter broadcast.
            filteredMatches = broadcastFilter.FilterBroadcasts(videosToProcess, testConfiguration:true);

            // Assert that filteredMatches has correct information.

            // Assert that the broadcast video got moved to processed.

            // 
        }
    }
}