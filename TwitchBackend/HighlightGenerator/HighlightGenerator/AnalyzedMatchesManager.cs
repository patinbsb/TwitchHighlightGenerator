using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    public static class AnalyzedMatchesManager
    {
        private static string AnalyzedMatchesPath = ConfigurationManager.AppSettings["AnalyzedMatchesPath"];
        private static string AnalyzedMatchesJson = "AnalyzedMatches.json";
        public static List<MatchMetrics> AnalyzedMatches { get; private set; } = new List<MatchMetrics>();

        // Loads in the filteredMatch list file. If a text version exists it will use this first to avoid unnessecasry processing.
        public static void LoadFromFiles()
        {
            var filteredMatches = FilteredMatchesManager.FilteredMatches;
            AnalyzedMatches = new List<MatchMetrics>();
            var folders = Directory.GetDirectories(AnalyzedMatchesPath);

            // LoadFromFiles.
            foreach (var filteredMatch in filteredMatches)
            {
                foreach (var folder in folders)
                {
                    if (folder.Contains(filteredMatch.Broadcast.Id.ToString()))
                    {
                        var files = Directory.GetFiles(folder);
                        foreach (var match in filteredMatch.Matches)
                        {
                            MatchMetrics metric;
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
                            if (!(ultimatesFound && killsFound && turretsFound && baronFound && dragonFound && inhibitorFound))
                            {
                                metric = new MatchAnalyzer().AnalyzeMatch(match, filteredMatch);
                            }
                            else
                            {
                                metric = new MatchAnalyzer().ParseMatch(killPath, ultimatePath, turretsPath, baronPath, dragonPath, inhibitorPath, match);
                            }
                            AnalyzedMatches.Add(metric);
                        }
                    }
                }
            }
        }

        public static void SaveToJson()
        {
            File.WriteAllText(AnalyzedMatchesPath + AnalyzedMatchesJson, JsonConvert.SerializeObject(AnalyzedMatches));
        }

        /// <summary>
        /// Adds analyzedmatch and creates a JSON copy for offline use.
        /// </summary>
        /// <param name="filteredMatch"></param>
        public static void AddAnalyzedMatch(MatchMetrics analyzedMatch)
        {
            if (!AnalyzedMatches.Contains(analyzedMatch))
            {
                AnalyzedMatches.Add(analyzedMatch);
                File.WriteAllText(AnalyzedMatchesPath + AnalyzedMatchesJson, JsonConvert.SerializeObject(AnalyzedMatches));
            }
            else
            {
                throw new Exception($"analyzedMatch was added where a duplicate already exists.");
            }
        }

        /// <summary>
        /// Adds analyzedmatches and creates a JSON copy for offline use.
        /// </summary>
        /// <param name="filteredMatches"></param>
        public static void AddAnalyzedMatches(List<MatchMetrics> matches)
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

        private static double convertVideoTimeToMatchOffset(double videoTime, Match match)
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
