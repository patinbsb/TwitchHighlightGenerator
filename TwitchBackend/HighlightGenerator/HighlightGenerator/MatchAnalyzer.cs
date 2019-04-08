using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class MatchAnalyzer
    {
        public static string ClipAnalyzerScriptPath = Helper.ScriptsPath + "Clip_Analyzer.py";

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
            List<MatchMetrics> matchMetrics = new List<MatchMetrics>();
            foreach (var filteredMatch in filteredMatches.Matches)
            {
                var matchPath = broadcastDirectory + filteredMatch.GetFileName();
                matchMetrics.Add(AnalyzeMatch(matchPath, filteredMatch));
            }
        }

        private MatchMetrics AnalyzeMatch(string matchPath, Match match)
        {
            
        }
    }

}
