using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Getting the root path of the project.
            string rootPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            while (rootPath.Contains("TwitchBackend"))
            {
                rootPath = Directory.GetParent(rootPath).FullName;
            }
            rootPath += "\\";
            string scriptPath = rootPath + "Scripts\\";
            ConfigurationManager.AppSettings["RootPath"] = rootPath;
            ConfigurationManager.AppSettings["ScriptsPath"] = scriptPath;
            string outputPath = ConfigurationManager.AppSettings["FilterVideoOutputPath"];

            //TODO this needs to be made machine agnostic
            var pythonPath = "C:\\Users\\patin_000\\Anaconda3\\python.exe";


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

            foreach (var video in videosToProcess)
            {
                string videoParam = video.Replace("\\", @"\\");
                string outputParam = outputPath.Replace("\\", @"\\");
                var videoFilterPath = (scriptPath + "Video_Filter.py").Replace("\\", @"\\");

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = pythonPath;
                start.Arguments = $"{videoFilterPath} '{videoParam}' '{outputParam}'";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;

                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        Console.Write(result);
                    }
                }
            }
        }
    }
}
