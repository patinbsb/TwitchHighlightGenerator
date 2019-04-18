using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    /// <summary>
    /// Responsible for maintaining a local offline reference to the matches the project has performed analysis on.
    /// Ensures no unnecessary duplicated efforts are made by tracking state.
    /// </summary>
    public static class AnalyzedMatchesManager
    {
        // Reference to offline Json file location.
        private static readonly string AnalyzedMatchesPath = ConfigurationManager.AppSettings["AnalyzedMatchesPath"];
        private static readonly string AnalyzedMatchesJson = "AnalyzedMatches.json";
        public static List<MatchMetricGroup> AnalyzedMatches { get; private set; } = new List<MatchMetricGroup>();

        /// <summary>
        /// Loads in the filteredMatch list file. If a local analysis file exists it will use this first to avoid unnecessary processing.
        /// </summary>
        public static void LoadFromFiles()
        {

            var filteredMatches = FilteredMatchesManager.FilteredMatches;
            AnalyzedMatches = new List<MatchMetricGroup>();
            var folders = Directory.GetDirectories(AnalyzedMatchesPath);

            // Go over each filtered match and look up their corresponding analysis files.
            foreach (var filteredMatch in filteredMatches)
            {
                foreach (var folder in folders)
                {
                    if (folder.Contains(filteredMatch.Broadcast.Id.ToString()))
                    {
                        // Analysis files found.
                        var files = Directory.GetFiles(folder);

                        // Go over each match and create a corresponding analysis object using the local analysis files.
                        // If no file found, perform analysis and create an object from scratch (and a corresponding analysis file group.).
                        foreach (var match in filteredMatch.Matches)
                        {
                            // Check everything requires exists.
                            MatchMetricGroup metrics;
                            bool killsFound = false,
                                turretsFound = false,
                                ultimatesFound = false,
                                baronFound = false,
                                dragonFound = false,
                                inhibitorFound = false;
                            string killPath = null, ultimatePath = null, turretsPath = null, baronPath = null, dragonPath = null, inhibitorPath = null;
                            foreach (var file in files)
                            {
                                if (file.Contains(match.GetFileName(false)))
                                {
                                    if (file.Contains("kills"))
                                    {
                                        killPath = file;
                                        killsFound = true;
                                    }

                                    if (file.Contains("ultimates"))
                                    {
                                        ultimatePath = file;
                                        ultimatesFound = true;
                                    }

                                    if (file.Contains("turrets"))
                                    {
                                        turretsPath = file;
                                        turretsFound = true;
                                    }

                                    if (file.Contains("baron"))
                                    {
                                        baronPath = file;
                                        baronFound = true;
                                    }

                                    if (file.Contains("dragon"))
                                    {
                                        dragonPath = file;
                                        dragonFound = true;
                                    }

                                    if (file.Contains("inhibitor"))
                                    {
                                        inhibitorPath = file;
                                        inhibitorFound = true;
                                    }
                                }
                            }
                            // If one or more metrics missing, (re)generate analysis info.
                            if (!(ultimatesFound && killsFound && turretsFound && baronFound && dragonFound && inhibitorFound))
                            {
                                metrics = new MatchAnalyzer().AnalyzeMatch(match, filteredMatch);
                            }
                            // Load analysis info from file.
                            else
                            {
                                metrics = new MatchAnalyzer().ParseMatch(killPath, ultimatePath, turretsPath, baronPath, dragonPath, inhibitorPath, match);
                            }
                            AnalyzedMatches.Add(metrics);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the current state of Analyzed matches locally.
        /// </summary>
        public static void SaveToJson()
        {
            File.WriteAllText(AnalyzedMatchesPath + AnalyzedMatchesJson, JsonConvert.SerializeObject(AnalyzedMatches));
        }

        /// <summary>
        /// Adds analyzed match and saves a Json file locally.
        /// </summary>
        /// <param name="analyzedMatch"></param>
        public static void AddAnalyzedMatch(MatchMetricGroup analyzedMatch)
        {
            if (!AnalyzedMatches.Contains(analyzedMatch))
            {
                AnalyzedMatches.Add(analyzedMatch);
                File.WriteAllText(AnalyzedMatchesPath + AnalyzedMatchesJson, JsonConvert.SerializeObject(AnalyzedMatches));
            }
            else
            {
                throw new Exception("analyzedMatch was added where a duplicate already exists.");
            }
        }

        /// <summary>
        /// Adds analyzed matches and saves a Json file locally.
        /// </summary>
        /// <param name="matches"></param>
        public static void AddAnalyzedMatches(List<MatchMetricGroup> matches)
        {
            foreach (var match in matches)
            {
                if (AnalyzedMatches.Contains(match))
                {
                    throw new Exception($"filteredMatch was added where a duplicate already exists. duplicate: {match.Match.GetFileName()}");
                }
                else
                {
                    AnalyzedMatches.Add(match);
                }
            }
            File.WriteAllText(AnalyzedMatchesPath + AnalyzedMatchesJson, JsonConvert.SerializeObject(AnalyzedMatches));
        }

        /// <summary>
        /// Because each match is a collection of 1 or more segments, we can account for the time skipping between segments and get a true time offset value.
        /// The offset value is with respect to the raw broadcast video file.
        /// </summary>
        /// <param name="videoTime"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        private static double ConvertVideoTimeToMatchOffset(double videoTime, Match match)
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
