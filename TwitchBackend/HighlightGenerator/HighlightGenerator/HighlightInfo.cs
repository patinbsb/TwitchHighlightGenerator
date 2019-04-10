using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class HighlightInfo
    {

        public double StartOffset;
        public double Length;
        public double Score;

        public HighlightInfo(double startOffset, double length, double score)
        {
            this.StartOffset = startOffset;
            this.Length = length;
            this.Score = score;
        }
    }
}
