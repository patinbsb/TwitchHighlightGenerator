using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    class Program
    {
        private static object lockProgress = new object();
        private static object lockBroadcast = new object();
        private static List<Tuple<string, string>> taskProgress = new List<Tuple<string, string>>();
        private static List<Broadcast> broadcasts = new List<Broadcast>();

        static void Main(string[] args)
        {
            // Here we setup the environment.
            // We step back from the project location until we reach the project's base folder and establish folder locations from there.

            // Getting the root path of the project.
            string rootPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            while (rootPath.Contains("TwitchBackend"))
            {
                rootPath = Directory.GetParent(rootPath).FullName;
            }

            rootPath += "\\";
            ConfigurationManager.AppSettings["RootPath"] = rootPath;

            // Configuring Script, Broadcast.
            string scriptsPath = rootPath + "Scripts\\";
            ConfigurationManager.AppSettings["ScriptsPath"] = scriptsPath;
            string broadcastsPath = rootPath + "Broadcasts\\";
            string analyzedMatchesPath = rootPath + "Analyzed Broadcasts\\";
            string tensorflowDataPath = rootPath + "TensorflowData\\";
            string highlightVideosPath = rootPath + "HighlightVideos\\";
            ConfigurationManager.AppSettings["BroadcastsPath"] = broadcastsPath;
            ConfigurationManager.AppSettings["AnalyzedMatchesPath"] = analyzedMatchesPath;
            ConfigurationManager.AppSettings["TensorflowDataPath"] = tensorflowDataPath;
            ConfigurationManager.AppSettings["HighlightVideosPath"] = highlightVideosPath;

            //TODO this needs to be made machine agnostic
            // We locate the python interpreter location.
            var pythonPath = "C:\\Users\\patin_000\\Anaconda3\\python.exe";
            var tensorflowPythonPath = "C:\\Users\\patin_000\\Anaconda3\\envs\\tf-gpu\\python.exe";
            ConfigurationManager.AppSettings["PythonInterpreterPath"] = pythonPath;
            ConfigurationManager.AppSettings["TensofrflowPythonInterpreterPath"] = tensorflowPythonPath;

            // We check the Twitch VOD folder for pending videos.
            string twitchVodsPath = ConfigurationManager.AppSettings["TwitchVodsPath"];

            // Initialise existing broadcast data from previous sessions.
            //FilteredMatchesManager.loadFromJson();

            // Check for new videos to process.
            var files = Directory.GetFiles(twitchVodsPath);
            List<string> videosToProcess = new List<string>();

            // Filter out non video files.
            foreach (var file in files)
            {
                if (file.EndsWith(".mp4"))
                {
                    videosToProcess.Add(file);
                }
            }

            //var filteredMatches = new BroadcastFilter().FilterBroadcasts(videosToProcess);

            //foreach (var filteredMatch in filteredMatches)
            //{
            //    if (!filteredMatch.isPopulated)
            //    {
            //        filteredMatch.PopulateMatchChatLogs();
            //    }
            //}



            //FilteredMatchesManager.AddFilteredMatches(filteredMatches);

            //var matchAnalyzer = new MatchAnalyzer();
            //var matchCollection = new List<List<MatchMetrics>>();

            //Console.WriteLine("");
            //Console.WriteLine("Analyzing matches.");
            //Console.WriteLine("");

            //foreach (var filteredMatch in FilteredMatchesManager.FilteredMatches)
            //{
            //    matchCollection.Add(matchAnalyzer.AnalyzeMatches(filteredMatch));
            //}

            //foreach (var collection in matchCollection)
            //{
            //    AnalyzedMatchesManager.AddAnalyzedMatches(collection);
            //}

            //Console.WriteLine("");
            //Console.WriteLine("Match analysis complete.");
            //Console.WriteLine("");


            FilteredMatchesManager.loadFromJson();
            AnalyzedMatchesManager.LoadFromFiles();
            AnalyzedMatchesManager.SaveToJson();

            var deepLearner = new DeepLearner();
            var highlightGenerator = new HighlightMaker();

            foreach (var match in AnalyzedMatchesManager.AnalyzedMatches)
            {
                if (!match.Match.IsInstantReplay)
                {
                    var highlightInfo = deepLearner.GetHighlightPeriod(match);
                    var highlightVideoPath = highlightGenerator.CreateHighlight(highlightInfo, match.Match);
                }
            }

            

            //var analyzedMatches = AnalyzedMatchesManager.AnalyzedMatches;

            //var highlights = new List<MatchMetrics>();
            //var trainingData = new List<MatchMetrics>();
            //foreach (var match in analyzedMatches)
            //{
            //    if (match.Match.IsInstantReplay)
            //    {
            //        highlights.Add(match);
            //    }

            //    if (match.Match.BroadcastId == 325392389 && !match.Match.IsInstantReplay &&
            //        (match.Match.Id == 6 || match.Match.Id == 7 || match.Match.Id == 5 || match.Match.Id == 4 || match.Match.Id == 3 || match.Match.Id == 2 || match.Match.Id == 1 ))
            //    {
            //        trainingData.Add(match);
            //    }
            //}


            //var deepLearner = new DeepLearner();

            //deepLearner.PrepareHighlightsForTensorFlow(highlights);

            //foreach (var match in trainingData)
            //{
            //    if (false)
            //    { deepLearner.PrepareMatchForTensorFlow(match, false);}
            //    else
            //    {
            //        deepLearner.PrepareMatchForTensorFlow(match, true);
            //    }
            //}



            //var matchAnalyzer = new MatchAnalyzer();
            //var matchCollection = new List<List<MatchMetrics>>();
            //foreach (var filteredMatch in FilteredMatchesManager.FilteredMatches)
            //{
            //    matchCollection.Add(matchAnalyzer.AnalyzeMatches(filteredMatch));
            //}

            //foreach (var collection in matchCollection)
            //{
            //    AnalyzedMatchesManager.AddAnalyzedMatches(collection);
            //}

            //Console.WriteLine("");
            //Console.WriteLine("Match analysis complete.");
            //Console.WriteLine("");

            //List<MatchMetrics> highlights = new List<MatchMetrics>();
            //List<MatchMetrics> matches = new List<MatchMetrics>();
            //foreach (var match in AnalyzedMatchesManager.AnalyzedMatches)
            //{
            //    if (match.Match.IsInstantReplay)
            //    {
            //        highlights.Add(match);
            //    }
            //    else
            //    {
            //        matches.Add(match);
            //    }
            //}

            /*

            BroadcastFilter broadcastFilter = new BroadcastFilter();

            var filteredMatchesToAdd = broadcastFilter.FilterBroadcasts(videosToProcess);

            Console.WriteLine("Loading in chat-log from database to filtered matches.");
            foreach (var filteredMatch in filteredMatchesToAdd)
            {
                filteredMatch.PopulateMatchChatLogs();
            }

            FilteredMatchesManager.AddFilteredMatches(filteredMatchesToAdd);

            // Analyze filtered matches.
            MatchAnalyzer analyzer = new MatchAnalyzer();

            List<List<MatchMetrics>> matchMetricsList = new List<List<MatchMetrics>>();

            foreach (var filteredMatches in filteredMatchesToAdd)
            {
                matchMetricsList.Add(analyzer.AnalyzeMatches(filteredMatches));
            }

            //TODO purge filtered match chatlogs on complete.

            //TODO analyze matches

            //TODO send analysis to deep learning script

            */

        }
    }
}
