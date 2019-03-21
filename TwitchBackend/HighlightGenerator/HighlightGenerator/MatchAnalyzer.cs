using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class MatchAnalyzer
    {
        public MatchAnalyzer(FilteredMatches filteredMatches, List<MatchMetric> killDifferences, List<MatchMetric> ultimateUsage, List<MatchMetric> chatRate)
        {
            FilteredMatches = filteredMatches;
            KillDifferences = killDifferences;
            UltimateUsage = ultimateUsage;
            ChatRate = chatRate;
        }

        public FilteredMatches FilteredMatches { get; set; }
        public List<MatchMetric> KillDifferences { get; set; }
        public List<MatchMetric> UltimateUsage { get; set; }
        public List<MatchMetric> ChatRate { get; set; }

        public List<MatchMetrics> AnalyzeMatches(FilteredMatches filteredMatches)
        {
            // Run pythonscript...

            return null;
        }
    }

}
