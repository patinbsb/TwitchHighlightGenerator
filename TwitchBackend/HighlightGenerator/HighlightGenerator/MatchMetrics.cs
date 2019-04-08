using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class MatchMetrics
    {
        public MatchMetrics(List<MatchMetric> killDifferences, List<MatchMetric> ultimateUsage, List<MatchMetric> chatRate, Match match)
        {
            KillDifferences = killDifferences;
            UltimateUsage = ultimateUsage;
            ChatRate = chatRate;
            Match = match;
        }

        public MatchMetrics(Match match)
        {
            KillDifferences = new List<MatchMetric>();
            UltimateUsage = new List<MatchMetric>();
            ChatRate = new List<MatchMetric>();
            Match = match;
        }

        public List<MatchMetric> KillDifferences;
        public List<MatchMetric> UltimateUsage;
        public List<MatchMetric> ChatRate;
        public Match Match;
    }
}
