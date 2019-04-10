using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class HighlightMaker
    {
        private static string HighlightScriptPath = ConfigurationManager.AppSettings["ScriptsPath"] + "HighlightGenerator.py";
        private static string HighlightVideosPath = ConfigurationManager.AppSettings["HighlightVideosPath"];
        private static string ProcessedVodsPath = Helper.TwitchVodsPath + "processed\\";

        public HighlightMaker()
        {

        }

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
                                "_" + match.BroadcastId + "_" + match.GetFileName(true);

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

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = Helper.PythonInterpreterPath;
            start.Arguments = $"\"{highlightScriptPathParam}\" \"{videoToProcessParam}\" \"{outputPathParam}\" \"{timeStart.ToString()}\" \"{timeEnd.ToString()}\"";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;

            using (Process process = Process.Start(start))
            {
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
