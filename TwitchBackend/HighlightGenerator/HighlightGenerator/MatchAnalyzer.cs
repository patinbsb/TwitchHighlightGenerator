using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    /// <summary>
    /// Responsible for analyzing the contents of the matches that a Broadcast is broken down into.
    /// There are several tracked metrics that the Clip_Analyzer Python script will generate during analysis.
    /// Can also load the files output previously analyzed matches from files into objects in memory.
    /// </summary>
    public class MatchAnalyzer
    {
        // Location of the script that performs our match analysis.
        public static string ClipAnalyzerScriptPath = Helper.ScriptsPath + "Clip_Analyzer.py";

        // Used to track the progress of the 1 to many match analysis tasks that are currently processing.
        private readonly object _lockProgress = new object();
        private readonly List<Tuple<string, string>> _taskProgress = new List<Tuple<string, string>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filteredMatches"></param>
        /// <returns></returns>
        public List<MatchMetricGroup> AnalyzeMatches(FilteredMatches filteredMatches)
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
            List<MatchMetricGroup> matchMetrics = new List<MatchMetricGroup>();
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

        public MatchMetricGroup AnalyzeMatch(Match match, FilteredMatches matches)
        {
            var broadcastDirectory = matches.GetDirectoryPath();
            var matchPath = broadcastDirectory + match.GetFileName();
            var outcome = RunAnalysisOnMatch(matchPath, match);

            return ParseMatch(outcome.Item1, outcome.Item2, outcome.Item3, outcome.Item4, outcome.Item5, outcome.Item6, match);
        }

        private (string, string, string, string, string, string) RunAnalysisOnMatch(string matchPath, Match match)
        {
            string videoFilterScriptPathParam = Helper.ConvertToPythonPath(ClipAnalyzerScriptPath);
            string videoToProcessParam = Helper.ConvertToPythonPath(matchPath);
            var analyzedPath = matchPath.Remove(matchPath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            analyzedPath = analyzedPath.Replace("Broadcasts", "AnalyzedBroadcasts");
            var killsPath = analyzedPath + match.GetFileName(false) + "_kills.txt";
            var ultimatesPath = analyzedPath + match.GetFileName(false) + "_ultimates.txt";
            var turretsPath = analyzedPath + match.GetFileName(false) + "_turrets.txt";
            var baronPath = analyzedPath + match.GetFileName(false) + "_baron.txt";
            var dragonPath = analyzedPath + match.GetFileName(false) + "_dragon.txt";
            var inhibitorPath = analyzedPath + match.GetFileName(false) + "_inhibitor.txt";
            Directory.CreateDirectory(analyzedPath);

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = Helper.PythonInterpreterPath,
                Arguments =
                    $"\"{videoFilterScriptPathParam}\" \"{videoToProcessParam}\" \"{Helper.ConvertToPythonPath(killsPath)}\" " +
                    $"\"{Helper.ConvertToPythonPath(ultimatesPath)}\" \"{Helper.ConvertToPythonPath(turretsPath)}\" \"{Helper.ConvertToPythonPath(baronPath)}\" " +
                    $"\"{Helper.ConvertToPythonPath(dragonPath)}\" \"{Helper.ConvertToPythonPath(inhibitorPath)}\"",
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
                            stream?.Close();
                        }

                        //file is not locked
                        return false;
                    }

                    // * Chose alternative approach, code kept for future consideration. *
                    //if (!IsFileLocked(new FileInfo(matchPath)))
                    //{
                    //    var csvPath = matchPath.Replace(".mp4", ".csv");
                    //    //Directory.Move(matchPath, matchPath.Replace(Helper.BroadcastsPath, Helper.AnalyzedBroadcastsPath));
                    //    //Directory.Move(csvPath, csvPath.Replace(Helper.BroadcastsPath, Helper.AnalyzedBroadcastsPath));
                    //}
                }
            }

            return (killsPath, ultimatesPath, turretsPath, baronPath, dragonPath, inhibitorPath);
        }

        public MatchMetricGroup ParseMatch(string killsFilePath, string ultimateFilePath, string turretsFilePath, string baronFilePath, string dragonFilePath, string inhibitorFilePath, Match filteredMatch)
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
                        kills.Add(double.Parse(r.ReadLine() ?? throw new InvalidOperationException()));
                    }
                }

                using (StreamReader r = new StreamReader(ultimateFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        ultimates.Add(double.Parse(r.ReadLine() ?? throw new InvalidOperationException()));
                    }
                }

                using (StreamReader r = new StreamReader(turretsFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        turrets.Add(double.Parse(r.ReadLine() ?? throw new InvalidOperationException()));
                    }
                }

                using (StreamReader r = new StreamReader(baronFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        barons.Add(double.Parse(r.ReadLine() ?? throw new InvalidOperationException()));
                    }
                }

                using (StreamReader r = new StreamReader(dragonFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        dragons.Add(double.Parse(r.ReadLine() ?? throw new InvalidOperationException()));
                    }
                }

                using (StreamReader r = new StreamReader(inhibitorFilePath))
                {
                    while (!r.EndOfStream)
                    {
                        inhibitors.Add(double.Parse(r.ReadLine() ?? throw new InvalidOperationException()));
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
                // Ultimates have a score of 1.0
                ultimateUsage.Add(new MatchMetric(ultimate, 1.0));
            }

            foreach (var turret in turrets)
            {
                // Turrets have a score of 1.0
                turretKills.Add(new MatchMetric(turret, 1.0));
            }

            foreach (var baron in barons)
            {
                // Barons have a score of 1.0
                baronKills.Add(new MatchMetric(baron, 1.0));
            }

            foreach (var dragon in dragons)
            {
                // Dragons have a score of 1.0
                dragonKills.Add(new MatchMetric(dragon, 1.0));
            }

            foreach (var inhibitor in inhibitors)
            {
                // Inhibitors have a score of 1.0
                inhibitorKills.Add(new MatchMetric(inhibitor, 1.0));
            }

            var startTime = filteredMatch.StartTime;
            foreach (var segment in filteredMatch.Segments)
            {
                foreach (var message in segment.ChatLog.Messages)
                {
                    var timeStamp = (message.Timestamp - startTime).TotalSeconds;
                    chatRate.Add(new MatchMetric(timeStamp, 1.0));
                }
            }

            return new MatchMetricGroup(killDifferences, ultimateUsage, turretKills, baronKills, dragonKills, inhibitorKills, chatRate, filteredMatch);
        }

        public double ConvertVideoTimeToMatchOffset(double videoTime, Match match)
        {
            var videoStart = match.Segments[0].StartTime;
            double convertedTime = videoStart;

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
                    return convertedTime + videoTime;
                }
            }

            throw new Exception("video time is greater than the length of the video.");

            return -1.00;
        }
    }

}
