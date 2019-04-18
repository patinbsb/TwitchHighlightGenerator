namespace HighlightGenerator
{
    /// <summary>
    /// Represents the location of a highlight in a particular Broadcast.
    /// </summary>
    public class HighlightInfo
    {

        // Offset from the Broadcast start.
        public double StartOffset;
        // Highlight length.
        public double Length;
        // The score this highlight was given by the Tensorflow predictor.
        public double Score;

        public HighlightInfo(double startOffset, double length, double score)
        {
            StartOffset = startOffset;
            Length = length;
            Score = score;
        }
    }
}
