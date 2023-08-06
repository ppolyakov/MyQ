namespace RobotCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Automated Cleaning Robot MyQ - Room Mapping");

            if (args.Length < 2)
            {
                Console.WriteLine(@"Usage: .\RobotCleaner.exe <source.json> <result.json>");
                return;
            }

            string inputFilePath = args[0];
            string outputFilePath = args[1];

            CleaningRobot cleaningRobot = new(inputFilePath, outputFilePath);
            cleaningRobot.Start();

            Console.ReadLine();
        }
    }
}