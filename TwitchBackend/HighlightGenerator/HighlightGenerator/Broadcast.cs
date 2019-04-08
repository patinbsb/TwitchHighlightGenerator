using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace HighlightGenerator
{
    public class Broadcast
    {
        public Broadcast(int id, DateTime startTime)
        {
            this.Id = id;
            this.StartTime = startTime;
        }
        public int Id { get; set; }
        public DateTime StartTime { get; set; }

        public string GetFilePath()
        {
            return ConfigurationManager.AppSettings["RootPath"] + "Broadcasts\\" + Id.ToString();
        }
    }
}
