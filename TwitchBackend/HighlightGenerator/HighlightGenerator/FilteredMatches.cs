using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightGenerator
{
    public class FilteredMatches
    {
        public FilteredMatches(List<Match> matches, Broadcast broadcast)
        {
            this.Matches = matches;
            this.Broadcast = broadcast;
        }

        public List<Match> Matches { get; set; }
        public Broadcast Broadcast { get; set; }

    }
}
