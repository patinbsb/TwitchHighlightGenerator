using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace HighlightGenerator
{
    public class HighlightMaker
    {
        private static readonly string HighlightScriptPath = ConfigurationManager.AppSettings["ScriptsPath"] + "HighlightGenerator.py";
        private static readonly string HighlightVideosPath = ConfigurationManager.AppSettings["HighlightVideosPath"];
        private static readonly string ProcessedVodsPath = Helper.TwitchVodsPath + "processed\\";

        public string CreateHighlight(HighlightInfo highlightInfo, Match match)
        {
            var vodFiles = Directory.GetFiles(ProcessedVodsPath);

            string videoToProcess = "";
            foreach (var vodFile in vodFiles)
            {
                if (vodFile.Contains(match.BroadcastId.ToString()))
                {
                    videoToProcess = vodFile;
                }
            }

            string outputPath = HighlightVideosPath + highlightInfo.Score.ToString("F2") + "_"  + match.StartTime.AddSeconds(highlightInfo.StartOffset).ToString("yyyyMMddHHmmss") + 
                                "_" + match.BroadcastId + "_" + match.GetFileName();

            double startTime = highlightInfo.StartOffset;

            RunGenerator(HighlightScriptPath, videoToProcess, outputPath, startTime, startTime + highlightInfo.Length);

            return outputPath;
        }

        private void RunGenerator(string highlightScript, string videoToProcess, string outputPath, double timeStart,
            double timeEnd)
        {
            string highlightScriptPathParam = ConvertToPythonPath(highlightScript);
            string videoToProcessParam = ConvertToPythonPath(videoToProcess);
            string outputPathParam = ConvertToPythonPath(outputPath);

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = Helper.PythonInterpreterPath,
                Arguments =
                    $"\"{highlightScriptPathParam}\" \"{videoToProcessParam}\" \"{outputPathParam}\" \"{timeStart.ToString(CultureInfo.InvariantCulture)}\" \"{timeEnd.ToString(CultureInfo.InvariantCulture)}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

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

        private string ConvertToPythonPath(string path)
        {
            return path.Replace(@"\", @"\\");
        }
    }
}
