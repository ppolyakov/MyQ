namespace RobotCleaner.Models
{
    public class RoomData
    {
        public string[,] Map { get; set; }
        public Position Start { get; set; }
        public int Battery { get; set; }
        public string[] Commands { get; set; }
    }
}
