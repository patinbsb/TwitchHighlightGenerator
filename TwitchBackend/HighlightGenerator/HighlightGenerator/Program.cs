using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Getting the root path of the project.
            string rootPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            while (rootPath.Contains("TwitchBackend"))
            {
                rootPath = Directory.GetParent(rootPath).FullName;
            }
            rootPath += "\\";
            ConfigurationManager.AppSettings["RootPath"] = rootPath;

            var test = new TwitchMetadata();
            var vid = test.GetTwitchVideoFromId(398605891);
            Console.Out.WriteLine(vid.RecordedDate.ToString());
        }
    }
}
