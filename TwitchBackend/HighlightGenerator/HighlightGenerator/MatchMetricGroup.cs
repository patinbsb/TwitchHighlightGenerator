using System.Collections.Generic;

namespace HighlightGenerator
{
    /// <summary>
    /// Responsible for representing all the match metrics for a particular match.
    /// </summary>
    public class MatchMetricGroup
    {
        public MatchMetricGroup(List<MatchMetric> killDifferences, List<MatchMetric> ultimateUsage, 
            List<MatchMetric> turretKills, List<MatchMetric> baronKills, List<MatchMetric> dragonKills, 
            List<MatchMetric> inhibitorKills, List<MatchMetric> chatRate, Match match)
        {
            KillDifferences = killDifferences;
            UltimateUsage = ultimateUsage;
            TurretKills = turretKills;
            BaronKills = baronKills;
            DragonKills = dragonKills;
            InhibitorKills = inhibitorKills;
            ChatRate = chatRate;
            Match = match;
        }

        public MatchMetricGroup(Match match)
        {
            KillDifferences = new List<MatchMetric>();
            UltimateUsage = new List<MatchMetric>();
            TurretKills = new List<MatchMetric>();
            BaronKills = new List<MatchMetric>();
            DragonKills = new List<MatchMetric>();
            InhibitorKills = new List<MatchMetric>();
            ChatRate = new List<MatchMetric>();
            Match = match;
        }

        public List<MatchMetric> KillDifferences;
        public List<MatchMetric> UltimateUsage;
        public List<MatchMetric> TurretKills;
        public List<MatchMetric> BaronKills;
        public List<MatchMetric> DragonKills;
        public List<MatchMetric> InhibitorKills;
        public List<MatchMetric> ChatRate;
        public Match Match;
    }
}
