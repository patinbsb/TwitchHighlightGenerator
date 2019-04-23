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
    /// <summary>
    /// Responsible for packaging instant-replays and matches for training the Tensorflow model and generating highlight predictions.
    /// </summary>
    public class NeuralNetController
    {
        // Configuring path locations.
        private readonly string _tensorflowPath = ConfigurationManager.AppSettings["TensorflowDataPath"];
        private readonly string _tensorflowPythonInterpreterPath =
            ConfigurationManager.AppSettings["TensorflowPythonInterpreterPath"];
        private string _deepLearnerScriptPath = ConfigurationManager.AppSettings["ScriptsPath"] + "DeepLearningModel.py";

        // The time range that the Neural Network will operate in.
        private readonly int _secondsChunkSize = 1;

        /// <summary>
        /// Uses the Tensorflow model to predict the most exciting moment.
        /// Returns metadata about the highlight.
        /// </summary>
        /// <param name="matchMetricGroup"></param>
        /// <returns></returns>
        public HighlightInfo GetHighlightPeriod(MatchMetricGroup matchMetricGroup, bool testConfiguration = false)
        {
            if (testConfiguration)
            {
                _deepLearnerScriptPath = ConfigurationManager.AppSettings["AltScriptsPath"] + "DeepLearningModel.py";
            }

            // Packages matchMetricGroup info into a chunked up form that Tensorflow can understand and creates a csv file for the Tensorflow python script to reference.
            var (matchPath, predictedDataPath) = PrepareMatchForTensorFlow(matchMetricGroup, false);

            // Runs the python script that outputs a csv file for the predictions the Tensorflow model made about the excitement level at a particular time in the match.
            GetHighlightInfo(matchPath, predictedDataPath);

            // Load in the predictions made by the model.
            List<string> predictedDataRaw;
            try
            {
                predictedDataRaw = File.ReadAllLines(_tensorflowPath + "Predictions\\" + predictedDataPath).ToList();
            }
            catch (Exception)
            {
                // Very rare edge case when multiple parallel uses of the model can cause a failure to predict.
                GetHighlightInfo(matchPath, predictedDataPath);
                predictedDataRaw = File.ReadAllLines(_tensorflowPath + "Predictions\\" + predictedDataPath).ToList();
            }
            Console.WriteLine(predictedDataPath + " Complete.");

            List<double> predictedData = new List<double>();
            List<double> matchOffset = new List<double>();

            var matchAnalyzer = new MatchAnalyzer();

            // Parse predicted data into relevant objects. reference is via an offset from the original raw Broadcast video start time.
            var counter = 0.0;
            foreach (var line in predictedDataRaw)
            {
                predictedData.Add(double.Parse(line));
                matchOffset.Add(matchAnalyzer.ConvertVideoTimeToMatchOffset(counter * 15, matchMetricGroup.Match));
                counter += 1;
            }

            // Find the most exciting period in the match and its score.
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

            // We offset the start by an additional 90 seconds to account for time slippage.
            return new HighlightInfo(matchOffset[highestScoreIndex] + 90, 40, highestScore);
        }

        /// <summary>
        /// Runs the Python Tensorflow script and gets it to predict the excitement of each moment.
        /// Script then outputs a csv of those excitement scores.
        /// </summary>
        /// <param name="matchPath"></param>
        /// <param name="predictedDataPath"></param>
        private void GetHighlightInfo(string matchPath, string predictedDataPath)
        {
            // Load script parameters.
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = _tensorflowPythonInterpreterPath,
                Arguments =
                    $"\"{Helper.ConvertToPythonPath(_deepLearnerScriptPath)}\" \"{Helper.ConvertToPythonPath(matchPath)}\" \"{predictedDataPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Run script.
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

        /// <summary>
        /// Updates the instant replays the Tensorflow model uses to train on.
        /// </summary>
        /// <param name="data"></param>
        public void PrepareInstantReplaysForTensorFlow(List<MatchMetricGroup> data)
        {
            // Set the matchMetricGroup to a single ordered list of metrics which Tensorflow can understand.
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

            // Write the list to csv. The script knows the location.
            var outputCsv = new StringBuilder();
            foreach (var line in result)
            {
                outputCsv.AppendLine(string.Join(",", line));
            }
            File.WriteAllText(_tensorflowPath + "highlights.csv", outputCsv.ToString());
        }

        /// <summary>
        /// Formats the MatchMetricGroup for a single match into a form that Tensorflow can process.
        /// Metrics are grouped into discrete time chunks and written to csv files.
        /// Can be configured to create a csv using manual training data which corresponds to the input match.
        /// </summary>
        /// <param name="matchMetricGroup"></param>
        /// <param name="training">If manual training data exists, process it for the Tensorflow model.</param>
        /// <returns>The location of the match csv and the corresponding expected location of the scripts output predictions.</returns>
        public (string, string) PrepareMatchForTensorFlow(MatchMetricGroup matchMetricGroup, bool training)
        {
            // Each metric is grouped into a series of set time chunks.
            var chunkedKillDifferences = DivideIntoTimeChunks(matchMetricGroup.KillDifferences, _secondsChunkSize);
            var chunkedUltimateUsage = DivideIntoTimeChunks(matchMetricGroup.UltimateUsage, _secondsChunkSize);
            var chunkedTurretKills = DivideIntoTimeChunks(matchMetricGroup.TurretKills, _secondsChunkSize);
            var chunkedBaronKills = DivideIntoTimeChunks(matchMetricGroup.BaronKills, _secondsChunkSize);
            var chunkedDragonKills = DivideIntoTimeChunks(matchMetricGroup.DragonKills, _secondsChunkSize);
            var chunkedInhibitorKills = DivideIntoTimeChunks(matchMetricGroup.InhibitorKills, _secondsChunkSize);

            // Discovering the longest list for ensuring that the output for Tensorflow has metric chunks of equal length.
            int highestListCount = 0;
            highestListCount = (chunkedKillDifferences.Count > highestListCount) ? chunkedKillDifferences.Count : highestListCount;
            highestListCount = (chunkedUltimateUsage.Count > highestListCount) ? chunkedUltimateUsage.Count : highestListCount;
            highestListCount = (chunkedTurretKills.Count > highestListCount) ? chunkedTurretKills.Count : highestListCount;
            highestListCount = (chunkedBaronKills.Count > highestListCount) ? chunkedBaronKills.Count : highestListCount;
            highestListCount = (chunkedDragonKills.Count > highestListCount) ? chunkedDragonKills.Count : highestListCount;
            highestListCount = (chunkedInhibitorKills.Count > highestListCount) ? chunkedInhibitorKills.Count : highestListCount;

            // Group chat rate into time chunks.
            var chunkedChatRate = DivideChatIntoTimeChunks(matchMetricGroup.ChatRate, highestListCount);

            // Fill our output list for Tensorflow. Ensure that shorter metric lists are padded with 0's to ensure all lists are equal length.
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

            // Write 2D list out to csv.
            var outputCsv = new StringBuilder();
            foreach (var line in output)
            {
                outputCsv.AppendLine(string.Join(",", line));
            }

            // Save location modifier.
            if (training)
            {
                // Extra csv file created for training data labels.
                var outputCount = output.Count;
                // Load and parse label data into a Tensorflow friendly format.
                var trainingDataRaw = LoadTrainingData(matchMetricGroup.Match);
                var trainingData = DivideTrainingDataIntoTimeChunks(trainingDataRaw, _secondsChunkSize);

                // Align data by padding out labels with 0's until at the length of the matchMetric data.
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

                // Write labels to csv.
                outputCsv = new StringBuilder();
                foreach (var line in trainingData)
                {
                    outputCsv.AppendLine(string.Join(",", line));
                }
                // Labels.
                File.WriteAllText(_tensorflowPath + "TrainingData\\" + matchMetricGroup.Match.BroadcastId + matchMetricGroup.Match.GetFileName(false) + "_training.csv", outputCsv.ToString());
                // MatchMetricGroup.
                File.WriteAllText(_tensorflowPath + "TrainingData\\" + matchMetricGroup.Match.BroadcastId + matchMetricGroup.Match.GetFileName(false) + ".csv", outputCsv.ToString());
            }
            else
            {
                // MatchMetricGroup.
                File.WriteAllText(_tensorflowPath + "EvaluationData\\" + matchMetricGroup.Match.BroadcastId + matchMetricGroup.Match.GetFileName(false) + ".csv", outputCsv.ToString());
            }

            // Locations of relevant files required by the Tensorflow Python script.
            var matchPath = matchMetricGroup.Match.BroadcastId + matchMetricGroup.Match.GetFileName(false) + ".csv";
            var predictedDataPath = matchMetricGroup.Match.BroadcastId + matchMetricGroup.Match.GetFileName(false) + "_prediction" + ".csv";
            return (matchPath, predictedDataPath);
        }

        /// <summary>
        /// Groups MatchMetricGroup into distinct time chunks.
        /// </summary>
        /// <param name="metric"></param>
        /// <param name="secondsChunkSize"></param>
        /// <returns>List of metric scores over time chunk size.</returns>
        private List<double> DivideIntoTimeChunks(List<MatchMetric> metric, int secondsChunkSize)
        {
            // Parameters for parsing.
            var output = new List<double>();
            bool endMet = false;
            var chunkRangeStart = 0;
            var chunkRangeEnd = chunkRangeStart + secondsChunkSize;

            // Get the latest metric.
            double maxTimeStamp = 0;
            foreach (var item in metric)
            {
                if (item.TimeStamp > maxTimeStamp)
                {
                    maxTimeStamp = item.TimeStamp;
                }
            }

            // Group metrics into time chunks.
            while (!endMet)
            {
                double count = 0;
                // Grouping metric score.
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

        /// <summary>
        /// Groups chat-log frequency into distinct time chunks.
        /// </summary>
        /// <param name="metric"></param>
        /// <param name="maxChunks"></param>
        /// <returns>List containing number of messages sent over time chunk size.</returns>
        private List<double> DivideChatIntoTimeChunks(List<MatchMetric> metric, int maxChunks)
        {
            // Handling edge case when no Chat-log exists for a match.
            if (metric == null)
            { return new List<double>();}

            // Parse parameter setting.
            var output = new List<double>();
            var timeRangeMin = metric[0].TimeStamp;
            var timeRangeMax = metric[metric.Count - 1].TimeStamp;
            var timeRange = timeRangeMax - timeRangeMin;
            var timeChunk = timeRange / maxChunks;

            // Group message frequency into time chunks.
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

        /// <summary>
        /// Loads the labeled data for a particular match into memory.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private List<List<double>> LoadTrainingData(Match match)
        {
            // Load file.
            string filePath = _tensorflowPath + "unprocessed\\" + match.BroadcastId + "_" + match.GetFileName(false) + ".txt";
            var text = File.ReadAllText(filePath);

            // Parse labels.
            Regex pattern = new Regex("(\\d+\\.\\d+), (\\d+\\.\\d+)");
            var matches = pattern.Matches(text);

            // Load labels into memory.
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

        /// <summary>
        /// Groups labeled data into distinct time chunks.
        /// </summary>
        /// <param name="trainingData"></param>
        /// <param name="secondsChunkSize"></param>
        /// <returns></returns>
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
    }
}
