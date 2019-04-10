using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace HighlightGenerator
{
    class DeepLearner
    {
        private string tensorflowPath = ConfigurationManager.AppSettings["TensorflowDataPath"];

        private string tensorflowPythonInterpreterPath =
            ConfigurationManager.AppSettings["TensofrflowPythonInterpreterPath"];
        private string deepLearnerScriptPath = ConfigurationManager.AppSettings["ScriptsPath"] + "DeepLearningModel.py";
        private int secondsChunkSize = 1;

        public DeepLearner()
        {

        }

        public HighlightInfo GetHighlightPeriod(MatchMetrics match)
        {
            PrepareMatchForTensorFlow(match, false);

            var matchPath = match.Match.BroadcastId + match.Match.GetFileName(false) + ".csv";
            var predictedDataPath = match.Match.BroadcastId + match.Match.GetFileName(false) + ".csv";

            GetHighlightInfo(matchPath, predictedDataPath);

            List<string> predictedDataRaw;

            try
            {
                predictedDataRaw = File.ReadAllLines(tensorflowPath + "Predictions\\" + predictedDataPath).ToList();
            }
            catch (Exception e)
            {
                GetHighlightInfo(matchPath, predictedDataPath);
                predictedDataRaw = File.ReadAllLines(tensorflowPath + "Predictions\\" + predictedDataPath).ToList();
            }

             

            Console.WriteLine(predictedDataPath + " Complete.");


            List<double> predictedData = new List<double>();
            List<double> matchOffset = new List<double>();
            var matchAnalyzer = new MatchAnalyzer();

            var counter = 0.0;
            foreach (var line in predictedDataRaw)
            {
                predictedData.Add(double.Parse(line));
                matchOffset.Add(matchAnalyzer.convertVideoTimeToMatchOffset(counter * 15, match.Match));
                counter += 1;
            }

            var highestScore = 0.0;
            var index = 0;
            var highestScoreIndex = 0;
            foreach (var score in predictedData)
            {
                if (score > highestScore)
                {
                    highestScore = score;
                    highestScoreIndex = index;
                }

                index += 1;
            }


            return new HighlightInfo(matchOffset[highestScoreIndex] + 90, 40, highestScore);
        }

        private void GetHighlightInfo(string matchPath, string predictedDataPath)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = tensorflowPythonInterpreterPath;
            start.Arguments = $"\"{ConvertToPythonPath(deepLearnerScriptPath)}\" \"{ConvertToPythonPath(matchPath)}\" \"{predictedDataPath}\"";
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
                }
            }
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
                result.Add(highlightCompiled);
                highlightCompiled = new List<double>();
            }

            var outputCsv = new StringBuilder();

            foreach (var line in result)
            {
                outputCsv.AppendLine(string.Join(",", line));
            }

            File.WriteAllText(tensorflowPath + "highlights.csv", outputCsv.ToString());
        }

        public void PrepareMatchForTensorFlow(MatchMetrics match, bool training)
        {
            var chunkedKillDifferences = divideIntoTimeChunks(match.KillDifferences, match.Match, secondsChunkSize);
            var chunkedUltimateUsage = divideIntoTimeChunks(match.UltimateUsage, match.Match, secondsChunkSize);
            var chunkedTurretKills = divideIntoTimeChunks(match.TurretKills, match.Match, secondsChunkSize);
            var chunkedBaronKills = divideIntoTimeChunks(match.BaronKills, match.Match, secondsChunkSize);
            var chunkedDragonKills = divideIntoTimeChunks(match.DragonKills, match.Match, secondsChunkSize);
            var chunkedInhibitorKills = divideIntoTimeChunks(match.InhibitorKills, match.Match, secondsChunkSize);

            int highestListCount = 0;

            highestListCount = (chunkedKillDifferences.Count > highestListCount) ? chunkedKillDifferences.Count : highestListCount;
            highestListCount = (chunkedUltimateUsage.Count > highestListCount) ? chunkedUltimateUsage.Count : highestListCount;
            highestListCount = (chunkedTurretKills.Count > highestListCount) ? chunkedTurretKills.Count : highestListCount;
            highestListCount = (chunkedBaronKills.Count > highestListCount) ? chunkedBaronKills.Count : highestListCount;
            highestListCount = (chunkedDragonKills.Count > highestListCount) ? chunkedDragonKills.Count : highestListCount;
            highestListCount = (chunkedInhibitorKills.Count > highestListCount) ? chunkedInhibitorKills.Count : highestListCount;

            var chunkedChatRate = divideChatIntoTimeChunks(match.ChatRate, match.Match, highestListCount);


            var matchCompiled = new List<double>();
            var output = new List<List<double>>();

            for (int i = 0; i < highestListCount; i++)
            {
                if (i < chunkedKillDifferences.Count)
                { matchCompiled.Add(chunkedKillDifferences[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i < chunkedUltimateUsage.Count)
                { matchCompiled.Add(chunkedUltimateUsage[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i < chunkedTurretKills.Count)
                { matchCompiled.Add(chunkedTurretKills[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i < chunkedBaronKills.Count)
                { matchCompiled.Add(chunkedBaronKills[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i < chunkedDragonKills.Count)
                { matchCompiled.Add(chunkedDragonKills[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i < chunkedInhibitorKills.Count)
                { matchCompiled.Add(chunkedInhibitorKills[i]); }
                else
                { matchCompiled.Add(0.0); }

                if (i < chunkedChatRate.Count)
                { matchCompiled.Add(chunkedChatRate[i]); }
                else
                { matchCompiled.Add(0.0); }

                output.Add(matchCompiled);
                matchCompiled = new List<double>();
            }

            var outputCount = output.Count;

            var outputCsv = new StringBuilder();

            foreach (var line in output)
            {
                outputCsv.AppendLine(string.Join(",", line));
            }

            if (training)
            {
                File.WriteAllText(tensorflowPath + "TrainingData\\" + match.Match.BroadcastId + match.Match.GetFileName(false) + ".csv", outputCsv.ToString());
            }
            else
            {
                File.WriteAllText(tensorflowPath + "EvaluationData\\" + match.Match.BroadcastId + match.Match.GetFileName(false) + ".csv", outputCsv.ToString());
            }

            //TODO ensure test data and labels are of the same length.

            if (training)
            {
                var trainingDataRaw = LoadTrainingData(match.Match);

                var trainingData = DivideTrainingDataIntoTimeChunks(trainingDataRaw, secondsChunkSize);



                bool linedUp = false;

                while (!linedUp)
                {
                    if (outputCount > trainingData.Count)
                    {
                        trainingData.Add(0.0);
                    }
                    else if (outputCount < trainingData.Count)
                    {
                        trainingData.RemoveAt(trainingData.Count - 1);
                    }
                    else
                    {
                        linedUp = true;
                    }

                }

                outputCsv = new StringBuilder();

                foreach (var line in trainingData)
                {
                    outputCsv.AppendLine(string.Join(",", line));
                }


                File.WriteAllText(tensorflowPath + "TrainingData\\" + match.Match.BroadcastId + match.Match.GetFileName(false) + "_training.csv", outputCsv.ToString());
            }


        }

        private List<double> divideIntoTimeChunks(List<MatchMetric> metric, Match match, int secondsChunkSize)
        {
            var output = new List<double>();
            bool endMet = false;
            var chunkRangeStart = 0;
            var chunkRangeEnd = chunkRangeStart + secondsChunkSize;

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

        private List<double> divideChatIntoTimeChunks(List<MatchMetric> metric, Match match, int maxChunks)
        {
            if (metric == null)
            { return new List<double>();}

            var output = new List<double>();
            bool endMet = false;

            var itemsPerChunk = metric.Count / maxChunks;
            var timeRangeMin = metric[0].TimeStamp;
            var timeRangeMax = metric[metric.Count - 1].TimeStamp;

            var timeRange = timeRangeMax - timeRangeMin;
            var timeChunk = timeRange / maxChunks;

            for (int i = 0; i < maxChunks; i++)
            {
                var timeChunkMin = timeRangeMin + timeChunk * i;
                var timeChunkMax = timeRangeMin + timeChunk * (i + 1);

                double count = 0;
                foreach (var message in metric)
                {
                    if (message.TimeStamp > timeChunkMin && message.TimeStamp < timeChunkMax)
                    {
                        count += message.Score;
                    }
                }
                output.Add(count);
            }

            return output;
        }

        private List<List<double>> LoadTrainingData(Match match)
        {
            string filePath = tensorflowPath + "unprocessed\\" + match.BroadcastId + "_" + match.GetFileName(false) + ".txt";
            var text = File.ReadAllText(filePath);

            Regex pattern = new Regex("(\\d+\\.\\d+), (\\d+\\.\\d+)");

            var matches = pattern.Matches(text);

            var output = new List<List<double>>();
            var line = new List<double>();
            foreach (System.Text.RegularExpressions.Match regexMatch in matches)
            {
                line.Add(double.Parse(regexMatch.Groups[1].Value));
                line.Add(double.Parse(regexMatch.Groups[2].Value));
                output.Add(line);
                line = new List<double>();
            }

            return output;
        }

        private List<double> DivideTrainingDataIntoTimeChunks(List<List<double>> trainingData,
            int secondsChunkSize)
        {
            var output = new List<double>();
            bool endMet = false;
            var chunkRangeStart = 0;
            var chunkRangeEnd = chunkRangeStart + secondsChunkSize;

            double maxTimeStamp = 0;
            foreach (var item in trainingData)
            {
                if (item[0] > maxTimeStamp)
                {
                    maxTimeStamp = item[0];
                }
            }

            while (!endMet)
            {
                int count = 0;
                double score = 0;
                foreach (var item in trainingData)
                {
                    if (item[0] > chunkRangeStart && item[0]< chunkRangeEnd)
                    {
                        score += item[1];
                        count++;
                    }
                }

                score = score / count;
                output.Add(score);
                chunkRangeStart += secondsChunkSize;
                chunkRangeEnd += secondsChunkSize;

                if (chunkRangeStart > maxTimeStamp)
                {
                    endMet = true;
                }
            }

            return output;
        }

        private string ConvertToPythonPath(string path)
        {
            return path.Replace(@"\", @"\\");
        }
    }
}
