using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace HighlightGenerator
{
    /// <summary>
    /// Takes in raw VOD videos. Filters them and adds their broadcast information to the LocalBroadcastManager.
    /// </summary>
    public class BroadcastFilter
    {
        public BroadcastFilter(string filterTemplatePath, string filterThreshold, string startingFrame, string framesToSkip,
            string convertToGreyscale, string secondsUntilTimeout, string secondsMinimumMatchLength)
        {
            FilterTemplatePath = filterTemplatePath;
            FilterThreshold = filterThreshold;
            StartingFrame = startingFrame;
            FramesToSkip = framesToSkip;
            ConvertToGreyscale = convertToGreyscale;
            SecondsUntilTimeout = secondsUntilTimeout;
            SecondsMinimumMatchLength = secondsMinimumMatchLength;
        }

        /// <summary>
        /// Load using default configuration values.
        /// </summary>
        public BroadcastFilter()
        {
            FilterTemplatePath = ConfigurationManager.AppSettings["FilterTemplatePath"];
            FilterThreshold = ConfigurationManager.AppSettings["FilterThreshold"];
            StartingFrame = ConfigurationManager.AppSettings["StartingFrame"];
            FramesToSkip = ConfigurationManager.AppSettings["FramesToSkip"];
            ConvertToGreyscale = ConfigurationManager.AppSettings["ConvertToGreyscale"];
            SecondsUntilTimeout = ConfigurationManager.AppSettings["SecondsUntilTimeout"];
            SecondsMinimumMatchLength = ConfigurationManager.AppSettings["SecondsMinimumMatchLength"];
        }

        // Python script parameters.
        public string FilterTemplatePath { get; set; }
        public string SecondsMinimumMatchLength { get; set; }
        public string SecondsUntilTimeout { get; set; }
        public string ConvertToGreyscale { get; set; }
        public string FramesToSkip { get; set; }
        public string StartingFrame { get; set; }
        public string FilterThreshold { get; set; }

        // Python script location.
        public string VideoFilterScriptPath = Helper.ScriptsPath + "Video_Filter.py";

        // For monitoring the progress of multiple threads and updating the user.
        private readonly object _lockProgress = new object();
        private object _lockBroadcast = new object();
        private readonly List<Tuple<string, string>> _taskProgress = new List<Tuple<string, string>>();

        /// <summary>
        /// Processes raw VOD files. Saves their broadcast information to the LocalBroadcastManager and returns the resulting filtered matches.
        /// </summary>
        /// <param name="videosToProcess"></param>
        /// <returns></returns>
        public List<FilteredMatches> FilterBroadcasts(List<string> videosToProcess, bool testConfiguration = false)
        {

            // We iterate over each VOD file, filter out gameplay, get their associated chat-log and create a metadata store in the form of a Broadcast list.
            List<Task> videoFilterTasks = new List<Task>();
            List<Broadcast> broadcasts = new List<Broadcast>();
            bool skip = false;
            foreach (var video in videosToProcess)
            {
                Console.WriteLine($"Found unprocessed video: {video}");

                // Extracting metadata from video filename
                var videoInfo = video.Substring(video.LastIndexOf("\\", StringComparison.Ordinal)+ 1).Split('_');

                var recordedDate = videoInfo[0];
                var recordedTime = videoInfo[1];
                DateTime videoRecordedDateTime = DateTime.ParseExact(recordedDate + recordedTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);

                var videoId = int.Parse(videoInfo[2]);

                if (!testConfiguration)
                {
                    // Check if the Broadcast has already been filtered. Skip this broadcast if true.
                    foreach (var filteredMatch in FilteredMatchesManager.FilteredMatches)
                    {
                        if (filteredMatch.Broadcast.Id == videoId)
                        {
                            Console.WriteLine(video + " has already been processed before, skipping.");
                            skip = true;
                            break;
                        }
                    }
                    if (skip)
                    {
                        skip = false;
                        continue;
                    }
                }


                // Keep track of this broadcast by assigning a corresponding Broadcast object to it.
                broadcasts.Add(new Broadcast(videoId, videoRecordedDateTime));

                if (testConfiguration)
                {
                    // We put each filtered broadcast into its own test folder.
                    Directory.CreateDirectory(Helper.BroadcastsPath + $"{videoId}");
                    string outputPath = Helper.BroadcastsPath + $"{videoId}\\";
                }
                else
                {
                    // We put each filtered broadcast into its own folder.
                    Directory.CreateDirectory(Helper.TestPath + "Broadcasts\\" + $"{videoId}");
                    string outputPath = Helper.TestPath + "Broadcasts\\" + $"{videoId}\\";
                }



                // Queue the broadcast filter operation for future parallel filtering.
                videoFilterTasks.Add(new Task(() => FilterVideo(video, outputPath, FilterTemplatePath, FilterThreshold, StartingFrame,
                    FramesToSkip, ConvertToGreyscale, SecondsUntilTimeout, SecondsMinimumMatchLength)));
            }

            Console.WriteLine("");
            Console.WriteLine("Parsing chat-logs to local database.");
            Console.WriteLine("");

            // Loads in the Twitch chat logs for the day to our local SQL database.
            // These will be loaded in filtered by each match's time range to avoid unnecessary ram usage.
            foreach (var broadcast in broadcasts)
            {
                Console.WriteLine($"processing {broadcast.Id}'s chat-log");
                ChatLogParser.GenerateChatRows(broadcast);
            }

            Console.WriteLine("chat-log processing completed.");
            Console.WriteLine("");
            Console.WriteLine("Starting video filtering.");
            Console.WriteLine("");

            // Start filtering our queued videos.
            foreach (var task in videoFilterTasks)
            {
                task.Start();
            }

            // Progress tracking
            Regex percentageRegex = new Regex("\\d+%");
            double highestPercentage = 0.00;
            double prevReportedPercentage = 0.00;
            List<int> taskProgressTracker = new List<int>();

            // Wait for all videos to be fully processed.
            // Regularly filter and report information given by each python process.
            while (!Task.WaitAll(videoFilterTasks.ToArray(), 500))
            {
                Tuple<string, String> progress = null;
                lock (_lockProgress)
                {
                    // Take the latest report from a particular task.
                    if (_taskProgress.Count > 0)
                    {
                        progress = _taskProgress[0];
                        _taskProgress.Remove(progress);
                    }
                }

                if (progress != null)
                {
                    // Percentage process of a particular task.
                    if (progress.Item2.Contains("%"))
                    {
                        var match = percentageRegex.Match(progress.Item2);
                        var percentage = int.Parse(match.Value.Trim('%'));
                        taskProgressTracker.Add(percentage);
                        // Rolling average.
                        if (taskProgressTracker.Count > 5)
                        {
                            taskProgressTracker.RemoveAt(0);
                        }

                        // Make sure the average progress never goes down.
                        if (taskProgressTracker.Average() > Math.Ceiling(highestPercentage))
                        {
                            highestPercentage += 1;
                            if (taskProgressTracker.Average() > prevReportedPercentage)
                            {
                                // Reports every % or so.
                                Console.WriteLine(taskProgressTracker.Average() + "% complete.");
                            }
                            prevReportedPercentage = taskProgressTracker.Average();
                        }
                    }

                    // Report when a filtered video (match or highlight) has been created.
                    if (progress.Item2.Contains("video created"))
                    {
                        Console.WriteLine(progress.Item2);
                    }
                }
            }
            Console.WriteLine("All videos processed.");

            // Use the csv report generated by each python process to create a reference to each filtered match and highlight.
            var filteredMatches = BuildFilteredMatchesFromBroadcasts(broadcasts);

            return filteredMatches;
        }

        /// <summary>
        /// Thread safe method for filtering an input video to an output location using the video_filter.py script
        /// And the locally installed python interpreter.
        /// </summary>
        /// <param name="videoToProcess"></param>
        /// <param name="outputPath"></param>
        /// <param name="filterTemplatePath"></param>
        /// <param name="filterThreshold"></param>
        /// <param name="startingFrame"></param>
        /// <param name="framesToSkip"></param>
        /// <param name="convertToGreyscale"></param>
        /// <param name="secondsUntilTimeout"></param>
        /// <param name="secondsMinimumMatchLength"></param>
        public void FilterVideo(string videoToProcess, string outputPath, string filterTemplatePath, string filterThreshold, string startingFrame, string framesToSkip,
            string convertToGreyscale, string secondsUntilTimeout, string secondsMinimumMatchLength)
        {
            // We convert to a python friendly parameter input.
            string videoFilterScriptPathParam = Helper.ConvertToPythonPath(VideoFilterScriptPath);
            string videoToProcessParam = Helper.ConvertToPythonPath(videoToProcess);
            string outputPathParam = Helper.ConvertToPythonPath(outputPath);
            string filterTemplatePathParam = Helper.ConvertToPythonPath(filterTemplatePath);

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = Helper.PythonInterpreterPath,
                Arguments =
                    $"\"{videoFilterScriptPathParam}\" \"{videoToProcessParam}\" \"{outputPathParam}\" \"{filterTemplatePathParam}\" \"{filterThreshold}\" \"{startingFrame}\" " +
                    $"\"{framesToSkip}\" \"{convertToGreyscale}\" \"{secondsUntilTimeout}\" \"{secondsMinimumMatchLength}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Track the progress of the video filter.
            // When the filter is complete, move the raw broadcast video to a processed folder so it isn't confused for a new Broadcast to filter.
            using (Process process = Process.Start(start))
            {
                Debug.Assert(process != null, nameof(process) + " != null");
                using (StreamReader reader = process.StandardOutput)
                {
                    // Write out the print statements the Python script outputs for reporting to the user.
                    while (!reader.EndOfStream)
                    {
                        string result = reader.ReadLine();
                        lock (_lockProgress)
                        {
                            _taskProgress.Add(new Tuple<string, string>(videoToProcess, result));
                        }
                    }
                    Console.WriteLine($"{videoToProcess} complete.");

                    // We move processed videos so they are not processed again.

                    // Method to check if a file is in use before performing a move operation.
                    bool IsFileLocked(FileInfo file)
                    {
                        FileStream stream = null;

                        try
                        {
                            stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                        }
                        catch (IOException)
                        {
                            //the file is unavailable because it is:
                            //still being written to
                            //or being processed by another thread
                            //or does not exist (has already been processed)
                            return true;
                        }
                        finally
                        {
                            stream?.Close();
                        }

                        //file is not locked
                        return false;
                    }

                    // Move the file if we can.
                    if (!IsFileLocked(new FileInfo(videoToProcess)))
                    {
                        Directory.Move(videoToProcess, videoToProcess.Replace(Helper.TwitchVodsPath, Helper.TwitchVodsPath + "processed\\"));
                    }
                }
            }
        }

        /// <summary>
        /// Use the csv report generated by each python process to create a reference to each filtered match and highlight.
        /// </summary>
        /// <param name="broadcasts"></param>
        /// <returns></returns>
        public List<FilteredMatches> BuildFilteredMatchesFromBroadcasts(List<Broadcast> broadcasts)
        {
            List<FilteredMatches> filteredMatches = new List<FilteredMatches>();

            // Each broadcast's containing folder.
            var directories = Directory.EnumerateDirectories(Helper.BroadcastsPath).ToList();

            // We go over each broadcast we filtered and look for the corresponding files the filter process generated.
            foreach (var broadcast in broadcasts)
            {
                List<Match> matches = new List<Match>();
                foreach (var directory in directories)
                {
                    // Broadcast location found.
                    if (directory.Contains(broadcast.Id.ToString()))
                    {
                        // Look through each file in the location.
                        var broadcastFiles = Directory.GetFiles(Helper.BroadcastsPath + broadcast.Id + "\\");
                        foreach (var fileVideo in broadcastFiles)
                        {
                            List<MatchSegment> startEndTimes = new List<MatchSegment>();
                            bool isInstantReplay = false;
                            bool csvFound = false;

                            if (fileVideo.Contains(".mp4"))
                            {
                                // Found a match.

                                // We look at its corresponding csv for segment information.
                                foreach (var fileCsv in broadcastFiles)
                                {
                                    if (!fileCsv.Contains(".mp4") && fileCsv.Replace(".csv", ".mp4") == fileVideo)
                                    {
                                        csvFound = true;
                                        // Load in the data in the csv file.
                                        using (var reader = new StreamReader(fileCsv))
                                        {
                                            while (!reader.EndOfStream)
                                            {
                                                // Find match start and end times (segments).
                                                var lines = reader.ReadLine().Split(',');
                                                double segmentStart = double.Parse(lines[0]);
                                                double segmentEnd = double.Parse(lines[1]);

                                                // Create an empty chat log for now, this will be auto-populated later on.
                                                var chatLog = new ChatLog(new List<Message>());
                                                startEndTimes.Add(new MatchSegment(segmentStart, segmentEnd, chatLog));
                                            }
                                        }
                                    }
                                }

                                // Look at the match file itself for important meta-info.
                                if (csvFound)
                                {
                                    // Collecting the rest of the information to build the match object.
                                    Regex idRegex = new Regex("\\d+\\.mp4");
                                    Regex idRegex2 = new Regex("\\d+");

                                    var match = idRegex.Match(fileVideo);
                                    var matchNumber = idRegex2.Match(match.Value);

                                    // Check if this is a instant replay video or a match.
                                    // We use the highlight videos as a way to provide training information to the neural-network model.
                                    if (fileVideo.Contains("highlight"))
                                    {
                                        isInstantReplay = true;
                                    }

                                    // Add the match's time offset value to the broadcast's start time to get the match start time.
                                    DateTime startTime = broadcast.StartTime.AddSeconds(startEndTimes[0].StartTime);

                                    // Done!
                                    matches.Add(new Match(startTime, int.Parse(matchNumber.Value), broadcast.Id, isInstantReplay, startEndTimes));
                                }

                            }
                        }

                    }
                }
                filteredMatches.Add(new FilteredMatches(matches, broadcast));
            }

            return filteredMatches;
        }
    }
}
