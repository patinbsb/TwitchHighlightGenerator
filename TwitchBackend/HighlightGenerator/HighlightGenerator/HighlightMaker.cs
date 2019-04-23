using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace HighlightGenerator
{
    /// <summary>
    /// Handles the Python script which writes a highlight video from a segment of a Broadcast using the predictions of our Neural net.
    /// </summary>
    public class HighlightMaker
    {
        // Script and location of our already processed Broadcast's raw video file.
        private static readonly string HighlightScriptPath = ConfigurationManager.AppSettings["ScriptsPath"] + "HighlightGenerator.py";
        private static readonly string HighlightVideosPath = ConfigurationManager.AppSettings["HighlightVideosPath"];
        private static readonly string ProcessedVodsPath = Helper.TwitchVodsPath + "processed\\";

        /// <summary>
        /// Renders a highlight video using a time slice from a Broadcast's video file.
        /// </summary>
        /// <param name="highlightInfo"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public string CreateHighlight(HighlightInfo highlightInfo, Match match, bool testConfiguration = false)
        {
            string videoToProcess = "";
            string[] vodFiles;

            if (testConfiguration)
            {
                vodFiles = Directory.GetFiles(Helper.TwitchVodsPath);
            }
            else
            {
                // Find the Broadcast file.
                vodFiles = Directory.GetFiles(ProcessedVodsPath);
            }

            foreach (var vodFile in vodFiles)
            {
                if (vodFile.Contains(match.BroadcastId.ToString()))
                {
                    videoToProcess = vodFile;
                }
            }

            // Set the format of our output highlight video's name and its location.
            string outputPath = HighlightVideosPath + highlightInfo.Score.ToString("F2") + "_"  + match.StartTime.AddSeconds(highlightInfo.StartOffset).ToString("yyyyMMddHHmmss") + 
                                "_" + match.BroadcastId + "_" + match.GetFileName();

            // Run the Python script to generate our highlight.
            double startTime = highlightInfo.StartOffset;
            RunGenerator(HighlightScriptPath, videoToProcess, outputPath, startTime, startTime + highlightInfo.Length);

            return outputPath;
        }

        /// <summary>
        /// Handles feeding the parameter and running the Python script which handles highlight video generation.
        /// </summary>
        /// <param name="highlightScript"></param>
        /// <param name="videoToProcess"></param>
        /// <param name="outputPath"></param>
        /// <param name="timeStart"></param>
        /// <param name="timeEnd"></param>
        private void RunGenerator(string highlightScript, string videoToProcess, string outputPath, double timeStart,
            double timeEnd)
        {
            // Setting python friendly parameters.
            string highlightScriptPathParam = Helper.ConvertToPythonPath(highlightScript);
            string videoToProcessParam = Helper.ConvertToPythonPath(videoToProcess);
            string outputPathParam = Helper.ConvertToPythonPath(outputPath);

            // Configure our script.
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = Helper.PythonInterpreterPath,
                Arguments =
                    $"\"{highlightScriptPathParam}\" \"{videoToProcessParam}\" \"{outputPathParam}\" \"{timeStart.ToString(CultureInfo.InvariantCulture)}\" \"{timeEnd.ToString(CultureInfo.InvariantCulture)}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Wait for script to complete.
            using (Process process = Process.Start(start))
            {
                Debug.Assert(process != null, nameof(process) + " != null");
                using (StreamReader reader = process.StandardOutput)
                {
                    while (!reader.EndOfStream)
                    {
                        reader.ReadLine();
                    }
                    Console.WriteLine($"{outputPathParam} highlight generation complete.");
                }
            }
        }
    }
}
