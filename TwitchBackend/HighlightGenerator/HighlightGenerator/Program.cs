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
            string analyzedMatchesPath = rootPath + "AnalyzedBroadcasts\\";
            string tensorflowDataPath = rootPath + "TensorflowData\\";
            string highlightVideosPath = rootPath + "HighlightVideos\\";
            string twitchVodsPath = rootPath + "TwitchVods\\";
            ConfigurationManager.AppSettings["BroadcastsPath"] = broadcastsPath;
            ConfigurationManager.AppSettings["TwitchVodsPath"] = twitchVodsPath;
            ConfigurationManager.AppSettings["AnalyzedMatchesPath"] = analyzedMatchesPath;
            ConfigurationManager.AppSettings["TensorflowDataPath"] = tensorflowDataPath;
            ConfigurationManager.AppSettings["HighlightVideosPath"] = highlightVideosPath;
            ConfigurationManager.AppSettings["FilterTemplatePath"] = scriptsPath + "broadcastFilterTemplate.png";

            // Initialise existing broadcast data from previous sessions.
            FilteredMatchesManager.loadFromJson();

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

            var filteredMatches = new BroadcastFilter().FilterBroadcasts(videosToProcess);

            Console.WriteLine("");
            Console.WriteLine("Populating chatlogs for filtered matches");
            Console.WriteLine("");

            foreach (var filteredMatch in filteredMatches)
            {
                if (!filteredMatch.isPopulated)
                {
                    filteredMatch.PopulateMatchChatLogs();
                }
            }

            FilteredMatchesManager.AddFilteredMatches(filteredMatches);

            var matchAnalyzer = new MatchAnalyzer();
            var matchCollection = new List<List<MatchMetrics>>();

            Console.WriteLine("");
            Console.WriteLine("Analyzing matches.");
            Console.WriteLine("");

            foreach (var filteredMatch in FilteredMatchesManager.FilteredMatches)
            {
                matchCollection.Add(matchAnalyzer.AnalyzeMatches(filteredMatch));
            }

            foreach (var collection in matchCollection)
            {
                AnalyzedMatchesManager.AddAnalyzedMatches(collection);
            }

            Console.WriteLine("");
            Console.WriteLine("Match analysis complete.");
            Console.WriteLine("");

            AnalyzedMatchesManager.LoadFromFiles();
            AnalyzedMatchesManager.SaveToJson();

            Console.WriteLine("");
            Console.WriteLine("Loading analyzed match info into Deep Learning Predictor.");
            Console.WriteLine("");

            var deepLearner = new DeepLearner();
            var highlightGenerator = new HighlightMaker();

            foreach (var match in AnalyzedMatchesManager.AnalyzedMatches)
            {
                if (!match.Match.IsInstantReplay)
                {
                    var highlightInfo = deepLearner.GetHighlightPeriod(match);
                    var highlightVideoPath = highlightGenerator.CreateHighlight(highlightInfo, match.Match);
                    Console.WriteLine($"Highlight video created at: {highlightVideoPath} for match: {match.Match.GetFileName(false)}");
                }
            }

            Console.WriteLine($"Processing complete: Go to {highlightVideosPath} to see highlight videos.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
