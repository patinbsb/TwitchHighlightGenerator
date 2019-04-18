namespace HighlightGenerator
{
    public class HighlightInfo
    {

        public double StartOffset;
        public double Length;
        public double Score;

        public HighlightInfo(double startOffset, double length, double score)
        {
            StartOffset = startOffset;
            Length = length;
            Score = score;
        }
    }
}
