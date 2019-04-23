using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using HighlightGenerator;

namespace HighlightGeneratorTests
{
    static class TestHelper
    {
        public static string rootPath;
        public static string scriptsPath;
        public static string broadcastsPath;
        public static string analyzedMatchesPath;
        public static string tensorflowDataPath;
        public static string highlightVideosPath;
        public static string twitchVodsPath;
        public static string filterTemplatePath;
        public static List<FilteredMatches> filteredMatches;
        public static List<MatchMetricGroup> analyzedMatches;

        public static void Initialise()
        {
            rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            while (rootPath.Contains("TwitchBackend"))
            {
                rootPath = Directory.GetParent(rootPath).FullName;
            }

            scriptsPath = rootPath + "\\Scripts\\";
            rootPath += "\\TestData\\";

            // Configuring Path locations.
            broadcastsPath = rootPath + "Broadcasts\\";
            analyzedMatchesPath = rootPath + "AnalyzedBroadcasts\\";
            tensorflowDataPath = rootPath + "TensorflowData\\";
            highlightVideosPath = rootPath + "HighlightVideos\\";
            twitchVodsPath = rootPath + "TwitchVods\\";
            filterTemplatePath = scriptsPath + "broadcastFilterTemplate.png";
            FilteredMatchesManager.LoadFromJson();
            AnalyzedMatchesManager.LoadFromFiles();
            filteredMatches = FilteredMatchesManager.FilteredMatches;
            analyzedMatches = AnalyzedMatchesManager.AnalyzedMatches;
        }
    }
}
