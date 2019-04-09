using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class MatchMetric
    {
        public MatchMetric(double timeStamp, double score)
        {
            this.TimeStamp = timeStamp;
            this.Score = score;
        }

        public double TimeStamp { get; set; }
        public double Score { get; set; }
    }
}
