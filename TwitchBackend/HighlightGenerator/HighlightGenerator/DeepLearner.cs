using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HighlightGenerator
{
    class DeepLearner
    {
        private readonly string _tensorflowPath = ConfigurationManager.AppSettings["TensorflowDataPath"];

        private readonly string _tensorflowPythonInterpreterPath =
            ConfigurationManager.AppSettings["TensorflowPythonInterpreterPath"];
        private readonly string _deepLearnerScriptPath = ConfigurationManager.AppSettings["ScriptsPath"] + "DeepLearningModel.py";
        private readonly int _secondsChunkSize = 1;

        public HighlightInfo GetHighlightPeriod(MatchMetrics match)
        {
            PrepareMatchForTensorFlow(match, false);

            var matchPath = match.Match.BroadcastId + match.Match.GetFileName(false) + ".csv";
            var predictedDataPath = match.Match.BroadcastId + match.Match.GetFileName(false) + ".csv";

            GetHighlightInfo(matchPath, predictedDataPath);

            List<string> predictedDataRaw;

            try
            {
                predictedDataRaw = File.ReadAllLines(_tensorflowPath + "Predictions\\" + predictedDataPath).ToList();
            }
            catch (Exception)
            {
                GetHighlightInfo(matchPath, predictedDataPath);
                predictedDataRaw = File.ReadAllLines(_tensorflowPath + "Predictions\\" + predictedDataPath).ToList();
            }

             

            Console.WriteLine(predictedDataPath + " Complete.");


            List<double> predictedData = new List<double>();
            List<double> matchOffset = new List<double>();
            var matchAnalyzer = new MatchAnalyzer();

            var counter = 0.0;
            foreach (var line in predictedDataRaw)
            {
                predictedData.Add(double.Parse(line));
                matchOffset.Add(matchAnalyzer.ConvertVideoTimeToMatchOffset(counter * 15, match.Match));
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
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = _tensorflowPythonInterpreterPath,
                Arguments =
                    $"\"{ConvertToPythonPath(_deepLearnerScriptPath)}\" \"{ConvertToPythonPath(matchPath)}\" \"{predictedDataPath}\"",
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

            File.WriteAllText(_tensorflowPath + "highlights.csv", outputCsv.ToString());
        }

        public void PrepareMatchForTensorFlow(MatchMetrics match, bool training)
        {
            var chunkedKillDifferences = DivideIntoTimeChunks(match.KillDifferences, _secondsChunkSize);
            var chunkedUltimateUsage = DivideIntoTimeChunks(match.UltimateUsage, _secondsChunkSize);
            var chunkedTurretKills = DivideIntoTimeChunks(match.TurretKills, _secondsChunkSize);
            var chunkedBaronKills = DivideIntoTimeChunks(match.BaronKills, _secondsChunkSize);
            var chunkedDragonKills = DivideIntoTimeChunks(match.DragonKills, _secondsChunkSize);
            var chunkedInhibitorKills = DivideIntoTimeChunks(match.InhibitorKills, _secondsChunkSize);

            int highestListCount = 0;

            highestListCount = (chunkedKillDifferences.Count > highestListCount) ? chunkedKillDifferences.Count : highestListCount;
            highestListCount = (chunkedUltimateUsage.Count > highestListCount) ? chunkedUltimateUsage.Count : highestListCount;
            highestListCount = (chunkedTurretKills.Count > highestListCount) ? chunkedTurretKills.Count : highestListCount;
            highestListCount = (chunkedBaronKills.Count > highestListCount) ? chunkedBaronKills.Count : highestListCount;
            highestListCount = (chunkedDragonKills.Count > highestListCount) ? chunkedDragonKills.Count : highestListCount;
            highestListCount = (chunkedInhibitorKills.Count > highestListCount) ? chunkedInhibitorKills.Count : highestListCount;

            var chunkedChatRate = DivideChatIntoTimeChunks(match.ChatRate, highestListCount);


            var matchCompiled = new List<double>();
            var output = new List<List<double>>();

            for (int i = 0; i < highestListCount; i++)
            {
                matchCompiled.Add(i < chunkedKillDifferences.Count ? chunkedKillDifferences[i] : 0.0);
                matchCompiled.Add(i < chunkedUltimateUsage.Count ? chunkedUltimateUsage[i] : 0.0);
                matchCompiled.Add(i < chunkedTurretKills.Count ? chunkedTurretKills[i] : 0.0);
                matchCompiled.Add(i < chunkedBaronKills.Count ? chunkedBaronKills[i] : 0.0);
                matchCompiled.Add(i < chunkedDragonKills.Count ? chunkedDragonKills[i] : 0.0);
                matchCompiled.Add(i < chunkedInhibitorKills.Count ? chunkedInhibitorKills[i] : 0.0);
                matchCompiled.Add(i < chunkedChatRate.Count ? chunkedChatRate[i] : 0.0);

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
                File.WriteAllText(_tensorflowPath + "TrainingData\\" + match.Match.BroadcastId + match.Match.GetFileName(false) + ".csv", outputCsv.ToString());
            }
            else
            {
                File.WriteAllText(_tensorflowPath + "EvaluationData\\" + match.Match.BroadcastId + match.Match.GetFileName(false) + ".csv", outputCsv.ToString());
            }

            if (training)
            {
                var trainingDataRaw = LoadTrainingData(match.Match);

                var trainingData = DivideTrainingDataIntoTimeChunks(trainingDataRaw, _secondsChunkSize);



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


                File.WriteAllText(_tensorflowPath + "TrainingData\\" + match.Match.BroadcastId + match.Match.GetFileName(false) + "_training.csv", outputCsv.ToString());
            }


        }

        private List<double> DivideIntoTimeChunks(List<MatchMetric> metric, int secondsChunkSize)
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

        private List<double> DivideChatIntoTimeChunks(List<MatchMetric> metric, int maxChunks)
        {
            if (metric == null)
            { return new List<double>();}

            var output = new List<double>();

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
            string filePath = _tensorflowPath + "unprocessed\\" + match.BroadcastId + "_" + match.GetFileName(false) + ".txt";
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
