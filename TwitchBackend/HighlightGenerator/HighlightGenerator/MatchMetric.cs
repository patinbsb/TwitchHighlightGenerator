namespace HighlightGenerator
{
    public class MatchMetric
    {
        public MatchMetric(double timeStamp, double score)
        {
            TimeStamp = timeStamp;
            Score = score;
        }

        public double TimeStamp { get; set; }
        public double Score { get; set; }
    }
}
