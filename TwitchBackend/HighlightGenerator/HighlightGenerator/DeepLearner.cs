using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HighlightGenerator
{
    class DeepLearner
    {
        private string tensorflowPath = ConfigurationManager.AppSettings["TensorflowDataPath"];

        public DeepLearner()
        {

        }

        public void PrepareHighlightsForTensorFlow(List<MatchMetrics> data)
        {
            var result = new List<List<double>>();
            var highlightCompiled = new List<double>();
            foreach (var highlight in data)
            {
                highlightCompiled.Add(highlight.KillDifferences.Sum(outp => outp.Score));
                highlightCompiled.Add(highlight.UltimateUsage.Sum(outp => outp.Score));
                highlightCompiled.Add(highlight.TurretKills.Sum(outp => outp.Score));
                highlightCompiled.Add(highlight.BaronKills.Sum(outp => outp.Score));
                highlightCompiled.Add(highlight.DragonKills.Sum(outp => outp.Score));
                highlightCompiled.Add(highlight.InhibitorKills.Sum(outp => outp.Score));
                highlightCompiled.Add(highlight.ChatRate.Sum(outp => outp.Score));
            }

            var outputCsv = new StringBuilder();

            foreach (var line in result)
            {
                outputCsv.AppendLine(string.Join(",", line));
            }

            File.WriteAllText(tensorflowPath + "highlights.csv", outputCsv.ToString());
        }

        public void PrepareMatchForTensorFlow(MatchMetrics match)
        {
            var chunkedKillDifferences = divideIntoTimeChunks(match.KillDifferences, match.Match, 10);
            var chunkedUltimateUsage = divideIntoTimeChunks(match.UltimateUsage, match.Match, 10);
            var chunkedTurretKills = divideIntoTimeChunks(match.TurretKills, match.Match, 10);
            var chunkedBaronKills = divideIntoTimeChunks(match.BaronKills, match.Match, 10);
            var chunkedDragonKills = divideIntoTimeChunks(match.DragonKills, match.Match, 10);
            var chunkedInhibitorKills = divideIntoTimeChunks(match.InhibitorKills, match.Match, 10);
            var chunkedChatRate = divideIntoTimeChunks(match.ChatRate, match.Match, 10);

            int highestListCount = 0;

            highestListCount = (chunkedKillDifferences.Count > highestListCount) ? chunkedKillDifferences.Count : highestListCount;
            highestListCount = (chunkedUltimateUsage.Count > highestListCount) ? chunkedUltimateUsage.Count : highestListCount;
            highestListCount = (chunkedTurretKills.Count > highestListCount) ? chunkedTurretKills.Count : highestListCount;
            highestListCount = (chunkedBaronKills.Count > highestListCount) ? chunkedBaronKills.Count : highestListCount;
            highestListCount = (chunkedDragonKills.Count > highestListCount) ? chunkedDragonKills.Count : highestListCount;
            highestListCount = (chunkedInhibitorKills.Count > highestListCount) ? chunkedInhibitorKills.Count : highestListCount;
            highestListCount = (chunkedChatRate.Count > highestListCount) ? chunkedChatRate.Count : highestListCount;

            var matchCompiled = new List<double>();
            var output = new List<List<double>>();

            for (int i = 0; i < highestListCount; i++)
            {
                if (i + 1 < chunkedKillDifferences.Count)
                { matchCompiled.Add(chunkedKillDifferences[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i + 1 < chunkedUltimateUsage.Count)
                { matchCompiled.Add(chunkedUltimateUsage[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i + 1 < chunkedTurretKills.Count)
                { matchCompiled.Add(chunkedTurretKills[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i + 1 < chunkedBaronKills.Count)
                { matchCompiled.Add(chunkedBaronKills[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i + 1 < chunkedDragonKills.Count)
                { matchCompiled.Add(chunkedDragonKills[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i + 1 < chunkedInhibitorKills.Count)
                { matchCompiled.Add(chunkedInhibitorKills[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i + 1 < chunkedChatRate.Count)
                { matchCompiled.Add(chunkedChatRate[i]); }
                else
                { matchCompiled.Add(0.0); }

                output.Add(matchCompiled);
                matchCompiled = new List<double>();
            }

            var outputCsv = new StringBuilder();

            foreach (var line in output)
            {
                outputCsv.AppendLine(string.Join(",", line));
            }

            File.WriteAllText(tensorflowPath + match.Match.BroadcastId + match.Match.GetFileName(false) + ".csv", outputCsv.ToString());
        }

        public List<double> divideIntoTimeChunks(List<MatchMetric> metric, Match match, int secondsChunkSize)
        {
            var output = new List<double>();
            bool endMet = false;
            var chunkRangeStart = 0;
            var chunkRangeEnd = secondsChunkSize;

            double maxTimeStamp = 0;
            foreach (var item in metric)
            {
                if (item.TimeStamp > maxTimeStamp)
                {
                    maxTimeStamp = item.TimeStamp;
                }
            }

            while (!endMet)
            {
                double count = 0;
                foreach (var item in metric)
                {
                    if (chunkRangeStart > item.TimeStamp)
                    if (item.TimeStamp > chunkRangeStart && item.TimeStamp < chunkRangeEnd)
                    {
                        count += item.Score;
                    }
                }
                output.Add(count);
                chunkRangeStart += secondsChunkSize;
                chunkRangeEnd += secondsChunkSize;

                if (chunkRangeStart > maxTimeStamp)
                {
                    endMet = true;
                }
            }

            return output;
        }
    }
}
