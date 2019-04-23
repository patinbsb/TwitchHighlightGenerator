using System;
using System.Collections.Generic;
using System.Configuration;
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
        public static string altScriptsPath;
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
            rootPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

            while (rootPath.Contains("TwitchBackend"))
            {
                rootPath = Directory.GetParent(rootPath).FullName;
            }

            scriptsPath = rootPath + "\\Scripts\\";
            rootPath += "\\TestData\\";
            altScriptsPath = rootPath + "\\Scripts\\";

            // Configuring Path locations.
            broadcastsPath = rootPath + "Broadcasts\\";
            analyzedMatchesPath = rootPath + "AnalyzedBroadcasts\\";
            tensorflowDataPath = rootPath + "TensorflowData\\";
            highlightVideosPath = rootPath + "HighlightVideos\\";
            twitchVodsPath = rootPath + "TwitchVods\\";
            filterTemplatePath = scriptsPath + "broadcastFilterTemplate.png";

            ConfigurationManager.AppSettings["ScriptsPath"] = scriptsPath;
            ConfigurationManager.AppSettings["AltScriptsPath"] = altScriptsPath;
            ConfigurationManager.AppSettings["BroadcastsPath"] = broadcastsPath;
            ConfigurationManager.AppSettings["TwitchVodsPath"] = twitchVodsPath;
            ConfigurationManager.AppSettings["AnalyzedMatchesPath"] = analyzedMatchesPath;
            ConfigurationManager.AppSettings["TensorflowDataPath"] = tensorflowDataPath;
            ConfigurationManager.AppSettings["HighlightVideosPath"] = highlightVideosPath;
            ConfigurationManager.AppSettings["FilterTemplatePath"] = scriptsPath + "broadcastFilterTemplate.png";
            ConfigurationManager.AppSettings["ChatLogPrefix"] = "https://overrustlelogs.net/Riotgames%20chatlog/";
            ConfigurationManager.AppSettings["PythonInterpreterPath"] = "C:\\\\Users\\\\patin_000\\\\Anaconda3\\\\envs\\\\tf-gpu\\\\python.exe";
            ConfigurationManager.AppSettings["TensorflowPythonInterpreterPath"] = "C:\\\\Users\\\\patin_000\\\\Anaconda3\\\\envs\\\\tf-gpu\\\\python.exe";
            ConfigurationManager.AppSettings["MySqlConnection"] = "server=localhost;user id=root; password=changeme;persistsecurityinfo=True;database=dsp";
            ConfigurationManager.AppSettings["FilterThreshold"] = "0.9";
            ConfigurationManager.AppSettings["StartingFrame"] = "178200";
            ConfigurationManager.AppSettings["FramesToSkip"] = "15";
            ConfigurationManager.AppSettings["ConvertToGreyscale"] = "False";
            ConfigurationManager.AppSettings["SecondsUntilTimeout"] = "120";
            ConfigurationManager.AppSettings["SecondsMinimumMatchLength"] = "600";

            FilteredMatchesManager.LoadFromJson();
            AnalyzedMatchesManager.LoadFromFiles();
            filteredMatches = FilteredMatchesManager.FilteredMatches;
            analyzedMatches = AnalyzedMatchesManager.AnalyzedMatches;
        }

        /// <summary>
        /// Prepares a path parameter for being passed into a python script.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ConvertToPythonPath(string path)
        {
            return path.Replace(@"\", @"\\");
        }
    }
}
