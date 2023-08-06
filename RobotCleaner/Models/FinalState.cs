namespace RobotCleaner.Models
{
    public class FinalState
    {
        public List<Dictionary<string, int>> Visited { get; set; }
        public List<Dictionary<string, int>> Cleaned { get; set; }
        public Position FinalPosition { get; set; }
        public int Battery { get; set; }
    }
}
