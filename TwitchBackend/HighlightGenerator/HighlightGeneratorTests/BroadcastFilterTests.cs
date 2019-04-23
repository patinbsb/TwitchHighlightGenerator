using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using HighlightGenerator;
using Helper = HighlightGenerator.Helper;
using HighlightGeneratorTests;

namespace Tests
{
    public class BroadcastFilterTests
    {
        private BroadcastFilter broadcastFilter;
        private List<FilteredMatches> filteredMatches;
        private List<MatchMetricGroup> matchCollection;
        private HighlightInfo highlightInfo;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestHelper.Initialise();
        }

        [SetUp]
        public void Setup()
        {
            
        }

        [Test, Order(1)]
        public void FilterBroadcastTest()
        {
            broadcastFilter = new BroadcastFilter();

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
            Assert.True(filteredMatches.Count == 1);
            Assert.True(filteredMatches[0].Broadcast.Id == 317396487);
            Assert.True(filteredMatches[0].Broadcast.StartTime.Ticks == 636740622290000000);
            Assert.True(filteredMatches[0].Matches.Count == 1);
            Assert.True(filteredMatches[0].Matches[0].Id == 1);
            Assert.True(!filteredMatches[0].Matches[0].IsInstantReplay);
            Assert.True(filteredMatches[0].Matches[0].Segments.Count == 6);
            Assert.True(filteredMatches[0].Matches[0].Segments[0].StartTime == 6071.9441215822417);
            Assert.True(filteredMatches[0].Matches[0].Segments[0].EndTime == 6232.6195459037581);

            var processedFileLocation = Helper.TwitchVodsPath + "processed\\" + videosToProcess[0].Split("\\").Last();
            // Assert that the Twitch VOD video got moved to processed.
            Assert.True(File.Exists(processedFileLocation));

            // Assert that the right matches, highlights and csv files were created.
            Assert.True(Directory.Exists(filteredMatches[0].GetDirectoryPath()));
            Assert.True(File.Exists(filteredMatches[0].GetDirectoryPath() + "match1.mp4"));
            Assert.True(File.Exists(filteredMatches[0].GetDirectoryPath() + "match1.csv"));

            // Move the Twitch VOD video back to its original location.
            File.Move(processedFileLocation, videosToProcess[0]);
        }

        [Test, Order(2)]
        public void PopulateMatchChatLogsTest()
        {
            // Load in filtered chat-log info into each filtered match.
            foreach (var filteredMatch in filteredMatches)
            {
                if (!filteredMatch.IsPopulated)
                {
                    filteredMatch.PopulateMatchChatLogs();
                }
            }

            // Assert that the filtered match has chat-logs loaded correctly.
            Assert.True(filteredMatches[0].IsPopulated);
            Assert.True(filteredMatches[0].Matches[0].IsPopulated);
            Assert.True(filteredMatches[0].Matches[0].Segments[0].IsPopulated);

            // No chat rows initially may be loaded in due to test conditions. We do this to double check.
            if (filteredMatches[0].Matches[0].Segments[0].ChatLog.Messages.Count == 0)
            {
                ChatLogParser.GenerateChatRows(filteredMatches[0].Broadcast);
                // Load in filtered chat-log info into each filtered match.
                foreach (var filteredMatch in filteredMatches)
                {
                    filteredMatch.IsPopulated = false;
                    foreach (var match in filteredMatch.Matches)
                    {
                        match.IsPopulated = false;

                        foreach (var segment in match.Segments)
                        {
                            segment.IsPopulated = false;
                        }
                    }
                }
            }

            // Load in filtered chat-log info into each filtered match.
            foreach (var filteredMatch in filteredMatches)
            {
                if (!filteredMatch.IsPopulated)
                {
                    filteredMatch.PopulateMatchChatLogs();
                }
            }

            Assert.True(filteredMatches[0].Matches[0].Segments[0].ChatLog.Messages.Count > 0);
        }

        [Test, Order(3)]
        public void AnalyzeMatchTest()
        {
            var matchAnalyzer = new MatchAnalyzer();
            matchCollection = matchAnalyzer.AnalyzeMatches(filteredMatches[0]);

            // Assert matchCollection has correct metrics.
            Assert.True(matchCollection[0].ChatRate.Count == 6402);
            Assert.True(matchCollection[0].BaronKills.Count == 1);
            Assert.True(matchCollection[0].InhibitorKills.Count == 1);
            Assert.True(matchCollection[0].KillDifferences.Count == 32);
            Assert.True(matchCollection[0].UltimateUsage.Count == 50);

            var baronFile = TestHelper.analyzedMatchesPath + matchCollection[0].Match.BroadcastId.ToString() +
                            "\\match1_baron.txt";
            // Assert files are generated correctly.
            Assert.True(File.Exists(baronFile));
        }

        [Test, Order(4)]
        public void GenerateHighlightInfoTest()
        {
            var deepLearner = new NeuralNetController();
            highlightInfo = deepLearner.GetHighlightPeriod(matchCollection[0], true);

            // Assert highlightInfo correct.
            Assert.True(highlightInfo.Score > 0.75);
            Assert.True(highlightInfo.StartOffset > 7400);
            Assert.True(File.Exists(TestHelper.tensorflowDataPath + "Predictions\\" + matchCollection[0].Match.BroadcastId + "match1_prediction.csv"));
        }

        [Test, Order(5)]
        public void CreateHighlightVideoTest()
        {
            var highlightGenerator = new HighlightMaker();
            var highlightPath = highlightGenerator.CreateHighlight(highlightInfo, matchCollection[0].Match, true);

            // Assert video made.
            Assert.True(File.Exists(highlightPath));
            Assert.True((new FileInfo(highlightPath).Length > 100000));

            // Remove highlight file.
            File.Delete(highlightPath);
        }
    }
}