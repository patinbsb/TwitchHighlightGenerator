using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization.Configuration;

namespace HighlightGenerator
{
    public static class Helper
    {
        public static string RootPath = ConfigurationManager.AppSettings["RootPath"];
        public static string ScriptsPath = ConfigurationManager.AppSettings["ScriptsPath"];
        public static string BroadcastsPath = ConfigurationManager.AppSettings["BroadcastsPath"];
        public static string AnalyzedBroadcastsPath = ConfigurationManager.AppSettings["AnalyzedBroadcastsPath"];
        public static string FilterTemplatePath = ConfigurationManager.AppSettings["FilterTemplatePath"];
        public static string TwitchVodsPath = ConfigurationManager.AppSettings["TwitchVodsPath"];
        public static string PythonInterpreterPath = ConfigurationManager.AppSettings["PythonInterpreterPath"];
        public static string MySqlConnection = ConfigurationManager.AppSettings["MySqlConnection"];
    }
}
