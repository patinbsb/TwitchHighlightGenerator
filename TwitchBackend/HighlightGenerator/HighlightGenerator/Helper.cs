using System.Configuration;

namespace HighlightGenerator
{
    /// <summary>
    /// Useful global variables for various classes to reference.
    /// </summary>
    public static class Helper
    {
        // The location of the root directory of the project.
        public static string RootPath = ConfigurationManager.AppSettings["RootPath"];
        // Location of the scripts directory.
        public static string ScriptsPath = ConfigurationManager.AppSettings["ScriptsPath"];
        // Location of the broadcasts directory.
        public static string BroadcastsPath = ConfigurationManager.AppSettings["BroadcastsPath"];
        // Location of the AnalyzedBroadcasts directory.
        public static string AnalyzedBroadcastsPath = ConfigurationManager.AppSettings["AnalyzedBroadcastsPath"];
        // Location of the image file which is used by the Video_Filter Python script to filter out non-gameplay from raw Broadcasts.
        public static string FilterTemplatePath = ConfigurationManager.AppSettings["FilterTemplatePath"];
        // Location of the TwitchVods directory.
        public static string TwitchVodsPath = ConfigurationManager.AppSettings["TwitchVodsPath"];
        // Location of the Python interpreter required to run our Python scripts.
        public static string PythonInterpreterPath = ConfigurationManager.AppSettings["PythonInterpreterPath"];
        // SQL connection string to gain database access.
        public static string MySqlConnection = ConfigurationManager.AppSettings["MySqlConnection"];
        // Used for when a test needs to override locations.
        public static string TestPath = ConfigurationManager.AppSettings["TestPath"];

        /// <summary>
        /// Prepares a path parameter for being passed into a python script.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ConvertToPythonPath(string path)
        {
            return path.Replace(@"\", @"\\");
        }
    }
}
