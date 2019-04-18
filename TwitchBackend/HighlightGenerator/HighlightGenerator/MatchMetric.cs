namespace HighlightGenerator
{
    /// <summary>
    /// Represents some measured feature of an analyzed match.
    /// Used for training and making predictions from our Tensorflow model.
    /// </summary>
    public class MatchMetric
    {
        public MatchMetric(double timeStamp, double score)
        {
            TimeStamp = timeStamp;
            Score = score;
        }

        // The offset time after a match start where the event occured.
        public double TimeStamp { get; set; }
        // The preassigned score for this particular event.
        public double Score { get; set; }
    }
}
