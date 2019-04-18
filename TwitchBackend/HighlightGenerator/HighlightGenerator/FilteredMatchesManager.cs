using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    /// <summary>
    /// Responsible for ensuring filtered matches are synchronized to their concrete file structure.
    /// Maintains an offline Json file for loading previously filtered matches into memory.
    /// </summary>
    public static class FilteredMatchesManager
    {
        // Offline Json file path configuration.
        private static readonly string BroadcastPath = ConfigurationManager.AppSettings["BroadcastsPath"];
        private static readonly string FilteredMatchesJson = "FilteredMatches.json";

        // Start with an empty filtered match list.
        public static List<FilteredMatches> FilteredMatches { get; set; } = new List<FilteredMatches>();

        /// <summary>
        /// Loads in the FilteredMatches Json file to memory.
        /// </summary>
        public static void LoadFromJson()
        {
            if (File.Exists(BroadcastPath + FilteredMatchesJson))
            {
                var filteredMatchJson = File.ReadAllText(BroadcastPath + FilteredMatchesJson);
                FilteredMatches = JsonConvert.DeserializeObject<List<FilteredMatches>>(filteredMatchJson) ?? new List<FilteredMatches>();
            }
            else
            {
                FilteredMatches = new List<FilteredMatches>();
            }
        }

        /// <summary>
        /// Saves all FilteredMatches to Json.
        /// </summary>
        public static void SaveToJson()
        {
            // Wont allow saving without loading.
            if ((FilteredMatches.Count > 0))
            {
                File.WriteAllText(BroadcastPath + FilteredMatchesJson, JsonConvert.SerializeObject(FilteredMatches));
            }
        }

        /// <summary>
        /// Adds filteredMatch to the filteredMatch list and creates a JSON copy for offline use.
        /// </summary>
        /// <param name="filteredMatch"></param>
        public static void AddFilteredMatch(FilteredMatches filteredMatch)
        {
            // Ensure no duplicate objects exist.
            if (!FilteredMatches.Contains(filteredMatch))
            {
                FilteredMatches.Add(filteredMatch);
                File.WriteAllText(BroadcastPath + FilteredMatchesJson, JsonConvert.SerializeObject(FilteredMatches));
            }
            else
            {
                throw new Exception($"filteredMatch was added where a duplicate already exists. duplicate ID: {filteredMatch.Broadcast.Id}");
            }
        }

        /// <summary>
        /// Adds filteredMatch to the filteredMatch list and creates a JSON copy for offline use.
        /// </summary>
        /// <param name="filteredMatches"></param>
        public static void AddFilteredMatches(List<FilteredMatches> filteredMatches)
        {
            foreach (var filteredMatch in filteredMatches)
            {
                // Ensure no duplicate objects exist.
                if (!FilteredMatches.Contains(filteredMatch))
                {
                    FilteredMatches.Add(filteredMatch);
                }
                else
                {
                    throw new Exception($"filteredMatch was added where a duplicate already exists. duplicate ID: {filteredMatch.Broadcast.Id}");
                }
            }
            File.WriteAllText(BroadcastPath + FilteredMatchesJson, JsonConvert.SerializeObject(FilteredMatches));
        }


    }
}
