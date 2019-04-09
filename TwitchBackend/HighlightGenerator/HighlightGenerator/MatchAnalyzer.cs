using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    public class MatchAnalyzer
    {
        public static string ClipAnalyzerScriptPath = Helper.ScriptsPath + "Clip_Analyzer.py";

        private object _lockProgress = new object();
        private List<Tuple<string, string>> _taskProgress = new List<Tuple<string, string>>();

        //public MatchAnalyzer(FilteredMatches filteredMatches, List<MatchMetric> killDifferences, List<MatchMetric> ultimateUsage, List<MatchMetric> chatRate)
        //{
        //    FilteredMatches = filteredMatches;
        //    KillDifferences = killDifferences;
        //    UltimateUsage = ultimateUsage;
        //    ChatRate = chatRate;
        //}

        public MatchAnalyzer()
        {

        }

        //public FilteredMatches FilteredMatches { get; set; }
        //public List<MatchMetric> KillDifferences { get; set; }
        //public List<MatchMetric> UltimateUsage { get; set; }
        //public List<MatchMetric> ChatRate { get; set; }

        public List<MatchMetrics> AnalyzeMatches(FilteredMatches filteredMatches)
        {
            var broadcastDirectory = filteredMatches.GetDirectoryPath();

            List<Task<(string, string, string, string, string, string)>> tasks = new List<Task<(string, string, string, string, string, string)>>();
            foreach (var filteredMatch in filteredMatches.Matches)
            {
                var matchPath = broadcastDirectory + filteredMatch.GetFileName();
                tasks.Add(new Task<(string, string, string, string, string, string)>(() => RunAnalysisOnMatch(matchPath, filteredMatch)));
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());

            int counter = 0;
            List<MatchMetrics> matchMetrics = new List<MatchMetrics>();
            foreach (var filteredMatch in filteredMatches.Matches)
            {
                var taskOutcome = tasks[counter].Result;
                var result = ParseMatch(taskOutcome.Item1, taskOutcome.Item2,taskOutcome.Item3,
                    taskOutcome.Item4, taskOutcome.Item5, taskOutcome.Item6, filteredMatch);
                matchMetrics.Add(result);
                counter++;
            }

            return matchMetrics;
        }

        public MatchMetrics AnalyzeMatch(Match match, FilteredMatches matches)
        {
            var broadcastDirectory = matches.GetDirectoryPath();
            var matchPath = broadcastDirectory + match.GetFileName();
            var outcome = RunAnalysisOnMatch(matchPath, match);

            return ParseMatch(outcome.Item1, outcome.Item2, outcome.Item3, outcome.Item4, outcome.Item5, outcome.Item6, match);
        }

        private (string, string, string, string, string, string) RunAnalysisOnMatch(string matchPath, Match match)
        {
            string videoFilterScriptPathParam = ConvertToPythonPath(ClipAnalyzerScriptPath);
            string videoToProcessParam = ConvertToPythonPath(matchPath);
            var analyzedPath = matchPath.Remove(matchPath.LastIndexOf("\\") + 1);
            analyzedPath = analyzedPath.Replace("Broadcasts", "Analyzed Broadcasts");
            var killsPath = analyzedPath + match.GetFileName(false) + "_kills.txt";
            var ultimatesPath = analyzedPath + match.GetFileName(false) + "_ultimates.txt";
            var turretsPath = analyzedPath + match.GetFileName(false) + "_turrets.txt";
            var baronPath = analyzedPath + match.GetFileName(false) + "_baron.txt";
            var dragonPath = analyzedPath + match.GetFileName(false) + "_dragon.txt";
            var inhibitorPath = analyzedPath + match.GetFileName(false) + "_inhibitor.txt";
            Directory.CreateDirectory(analyzedPath);

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = Helper.PythonInterpreterPath;
            start.Arguments = $"\"{videoFilterScriptPathParam}\" \"{videoToProcessParam}\" \"{ConvertToPythonPath(killsPath)}\" " +
                              $"\"{ConvertToPythonPath(ultimatesPath)}\" \"{ConvertToPythonPath(turretsPath)}\" \"{ConvertToPythonPath(baronPath)}\" " +
                              $"\"{ConvertToPythonPath(dragonPath)}\" \"{ConvertToPythonPath(inhibitorPath)}\"";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    while (!reader.EndOfStream)
                    {
                        string result = reader.ReadLine();
                        lock (_lockProgress)
                        {
                            _taskProgress.Add(new Tuple<string, string>(matchPath, result));
                        }
                    }
                    Console.WriteLine($"{matchPath} complete.");
                    // We move processed videos so they are not processed again.

                    bool IsFileLocked(FileInfo file)
                    {
                        FileStream stream = null;

                        try
                        {
                            stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                        }
                        catch (IOException)
                        {
                            //the file is unavailable because it is:
                            //still being written to
                            //or being processed by another thread
                            //or does not exist (has already been processed)
                            return true;
                        }
                        finally
                        {
                            if (stream != null)
                                stream.Close();
                        }

                        //file is not locked
                        return false;
                    }

                    if (!IsFileLocked(new FileInfo(matchPath)))
                    {
                        var csvPath = matchPath.Replace(".mp4", ".csv");
                        //TODO undo this.
                        //Directory.Move(matchPath, matchPath.Replace(Helper.BroadcastsPath, Helper.AnalyzedBroadcastsPath));
                        //Directory.Move(csvPath, csvPath.Replace(Helper.BroadcastsPath, Helper.AnalyzedBroadcastsPath));
                    }
                }
            }

            return (killsPath, ultimatesPath, turretsPath, baronPath, dragonPath, inhibitorPath);
        }

        public MatchMetrics ParseMatch(string killsFilePath, string ultimateFilePath, string turretsFilePath, string baronFilePath, string dragonFilePath, string inhibitorFilePath, Match filteredMatch)
        {

            List<double> kills = new List<double>();
            List<double> ultimates = new List<double>();
            List<double> turrets = new List<double>();
            List<double> barons = new List<double>();
            List<double> dragons = new List<double>();
            List<double> inhibitors = new List<double>();

            try
            {
                using (StreamReader r = new StreamReader(killsFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        kills.Add(double.Parse(r.ReadLine()));
                    }
                }

                using (StreamReader r = new StreamReader(ultimateFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        ultimates.Add(double.Parse(r.ReadLine()));
                    }
                }

                using (StreamReader r = new StreamReader(turretsFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        turrets.Add(double.Parse(r.ReadLine()));
                    }
                }

                using (StreamReader r = new StreamReader(baronFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        barons.Add(double.Parse(r.ReadLine()));
                    }
                }

                using (StreamReader r = new StreamReader(dragonFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        dragons.Add(double.Parse(r.ReadLine()));
                    }
                }

                using (StreamReader r = new StreamReader(inhibitorFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        inhibitors.Add(double.Parse(r.ReadLine()));
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Warn: {e.FileName} not found. Skipping.");
            }



            List<MatchMetric> killDifferences = new List<MatchMetric>();
            List<MatchMetric> ultimateUsage = new List<MatchMetric>();
            List<MatchMetric> turretKills = new List<MatchMetric>();
            List<MatchMetric> baronKills = new List<MatchMetric>();
            List<MatchMetric> dragonKills = new List<MatchMetric>();
            List<MatchMetric> inhibitorKills = new List<MatchMetric>();
            List<MatchMetric> chatRate = new List<MatchMetric>();

            //TODO revisit this scoring mechanic.
            foreach (var kill in kills)
            {
                // Kills have a score of 1.0
                killDifferences.Add(new MatchMetric(kill, 1.0));
            }

            foreach (var ultimate in ultimates)
            {
                ultimateUsage.Add(new MatchMetric(ultimate, 1.0));
            }

            foreach (var turret in turrets)
            {
                // Kills have a score of 1.0
                turretKills.Add(new MatchMetric(turret, 1.0));
            }

            foreach (var baron in barons)
            {
                // Kills have a score of 1.0
                baronKills.Add(new MatchMetric(baron, 1.0));
            }

            foreach (var dragon in dragons)
            {
                // Kills have a score of 1.0
                dragonKills.Add(new MatchMetric(dragon, 1.0));
            }

            foreach (var inhibitor in inhibitors)
            {
                // Kills have a score of 1.0
                inhibitorKills.Add(new MatchMetric(inhibitor, 1.0));
            }

            var totalMessages = filteredMatch.Segments.Sum(segment => segment.ChatLog.Messages.Count);
            var startTime = filteredMatch.StartTime;
            foreach (var segment in filteredMatch.Segments)
            {
                foreach (var message in segment.ChatLog.Messages)
                {
                    var timeStamp = (message.Timestamp - startTime).TotalSeconds;
                    chatRate.Add(new MatchMetric(timeStamp, 1.0));
                }
                //chatRate.Add(new MatchMetric((segment.StartTime + ((segment.EndTime - segment.StartTime) / 2)),
                //    (double)((double)segment.ChatLog.Messages.Count / (double)totalMessages)));
            }

            return new MatchMetrics(killDifferences, ultimateUsage, turretKills, baronKills, dragonKills, inhibitorKills, chatRate, filteredMatch);
        }

        private string ConvertToPythonPath(string path)
        {
            return path.Replace(@"\", @"\\");
        }

        private double convertVideoTimeToMatchOffset(double videoTime, Match match)
        {
            double convertedTime;
            var videoStart = match.Segments[0].StartTime;
            convertedTime = videoStart;

            List<double> segmentlengths = new List<double>();
            foreach (var segment in match.Segments)
            {
                var segmentLength = segment.EndTime - segment.StartTime;
                if (videoTime - segmentLength > 0)
                {
                    videoTime -= segmentLength;
                    convertedTime += segmentLength;
                }

                if (videoTime - segmentLength < 0)
                {
                    return convertedTime += videoTime;
                }
            }

            throw new Exception("video time is greater than the length of the video.");

            return -1.00;
        }
    }

}
