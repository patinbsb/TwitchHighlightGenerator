using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    public static class FilteredMatchesManager
    {
        private static string BroadcastPath = ConfigurationManager.AppSettings["BroadcastsPath"];
        private static string FilteredMatchesJson = "FilteredMatch.json";
        public static List<FilteredMatch> FilteredMatches { get; private set; }

        // Loads in the filteredMatch list file
        public static void loadFromJson()
        {
            if (File.Exists(BroadcastPath + FilteredMatchesJson))
            {
                var filteredMatchJson = File.ReadAllText(BroadcastPath + FilteredMatchesJson);
                FilteredMatches = JsonConvert.DeserializeObject<List<FilteredMatch>>(filteredMatchJson);
                if (FilteredMatches == null)
                {
                    FilteredMatches = new List<FilteredMatch>();
                }
            }
            else
            {
                FilteredMatches = new List<FilteredMatch>();
            }
        }

        public static void saveToJson()
        {
            File.WriteAllText(BroadcastPath + FilteredMatchesJson, JsonConvert.SerializeObject(FilteredMatches));
        }

        /// <summary>
        /// Adds filteredMatch to the filteredMatch list and creates a JSON copy for offline use.
        /// </summary>
        /// <param name="filteredMatch"></param>
        public static void AddFilteredMatch(FilteredMatch filteredMatch)
        {
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
        public static void AddFilteredMatches(List<FilteredMatch> filteredMatches)
        {
            foreach (var filteredMatch in filteredMatches)
            {
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
