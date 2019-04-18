using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace HighlightGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Here we setup the environment.
            // We step back from the project location until we reach the project's base folder and establish folder locations from there.

            // Getting the root path of the project.
            string rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            while (rootPath.Contains("TwitchBackend"))
            {
                rootPath = Directory.GetParent(rootPath).FullName;
            }

            rootPath += "\\";
            ConfigurationManager.AppSettings["RootPath"] = rootPath;

            // Configuring Path locations.
            string scriptsPath = rootPath + "Scripts\\";
            string broadcastsPath = rootPath + "Broadcasts\\";
            string analyzedMatchesPath = rootPath + "AnalyzedBroadcasts\\";
            string tensorflowDataPath = rootPath + "TensorflowData\\";
            string highlightVideosPath = rootPath + "HighlightVideos\\";
            string twitchVodsPath = rootPath + "TwitchVods\\";

            ConfigurationManager.AppSettings["ScriptsPath"] = scriptsPath;
            ConfigurationManager.AppSettings["BroadcastsPath"] = broadcastsPath;
            ConfigurationManager.AppSettings["TwitchVodsPath"] = twitchVodsPath;
            ConfigurationManager.AppSettings["AnalyzedMatchesPath"] = analyzedMatchesPath;
            ConfigurationManager.AppSettings["TensorflowDataPath"] = tensorflowDataPath;
            ConfigurationManager.AppSettings["HighlightVideosPath"] = highlightVideosPath;
            ConfigurationManager.AppSettings["FilterTemplatePath"] = scriptsPath + "broadcastFilterTemplate.png";

            // Initialise existing broadcast data from previous sessions.
            FilteredMatchesManager.LoadFromJson();

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

            // Process videos, stripping out non-gameplay elements.
            var filteredMatches = new BroadcastFilter().FilterBroadcasts(videosToProcess);

            Console.WriteLine("");
            Console.WriteLine("Populating chat-logs for filtered matches");
            Console.WriteLine("");

            // Load in filtered chat-log info into each filtered match.
            foreach (var filteredMatch in filteredMatches)
            {
                if (!filteredMatch.IsPopulated)
                {
                    filteredMatch.PopulateMatchChatLogs();
                }
            }

            // Save the newly discovered filtered match info to our manager.
            FilteredMatchesManager.AddFilteredMatches(filteredMatches);

            var matchAnalyzer = new MatchAnalyzer();
            var matchCollection = new List<List<MatchMetrics>>();

            Console.WriteLine("");
            Console.WriteLine("Analyzing matches.");
            Console.WriteLine("");

            // Go through each filtered match and analyze it for selected gameplay metrics (kills, ultimate usage, ect.).
            foreach (var filteredMatch in FilteredMatchesManager.FilteredMatches)
            {
                matchCollection.Add(matchAnalyzer.AnalyzeMatches(filteredMatch));
            }

            // Save the newly discovered analysis to our manager.
            foreach (var collection in matchCollection)
            {
                AnalyzedMatchesManager.AddAnalyzedMatches(collection);
            }

            Console.WriteLine("");
            Console.WriteLine("Match analysis complete.");
            Console.WriteLine("");

            // Ensuring a Json file is generated and in sync with the local files generated via analysis.
            AnalyzedMatchesManager.LoadFromFiles();
            AnalyzedMatchesManager.SaveToJson();

            Console.WriteLine("");
            Console.WriteLine("Loading analyzed match info into Deep Learning Predictor.");
            Console.WriteLine("");


            var deepLearner = new DeepLearner();
            var highlightGenerator = new HighlightMaker();

            // Finally we generate a highlight for each discovered match we find.
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
