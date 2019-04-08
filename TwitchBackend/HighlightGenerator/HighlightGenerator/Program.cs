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
            ConfigurationManager.AppSettings["BroadcastsPath"] = broadcastsPath;

            //TODO this needs to be made machine agnostic
            // We locate the python interpreter location.
            var pythonPath = "C:\\Users\\patin_000\\Anaconda3\\python.exe";
            ConfigurationManager.AppSettings["PythonInterpreterPath"] = pythonPath;

            // We check the Twitch VOD folder for pending videos.
            string twitchVodsPath = ConfigurationManager.AppSettings["TwitchVodsPath"];


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

            // Initialise existing broadcast data from previous sessions.
            FilteredMatchesManager.loadFromJson();

            BroadcastFilter broadcastFilter = new BroadcastFilter();

            var filteredMatchesToAdd = broadcastFilter.FilterBroadcasts(videosToProcess);

            Console.WriteLine("Loading in chat-log from database to filtered matches.");
            foreach (var filteredMatch in filteredMatchesToAdd)
            {
                filteredMatch.PopulateMatchChatLogs();
            }

            FilteredMatchesManager.AddFilteredMatches(filteredMatchesToAdd);


            //TODO purge filtered match chatlogs on complete.

            //TODO analyze matches

            //TODO send analysis to deep learning script

        }
    }
}
