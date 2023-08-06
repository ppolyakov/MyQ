using Newtonsoft.Json;
using RobotCleaner.Models;

namespace RobotCleaner
{
    public class CleaningRobot
    {
        public string InputJson { get; set; } = string.Empty;
        public string OutputJson { get; set; } = string.Empty;

        private List<Position> _visitedPositions = new List<Position>();
        private List<Position> _cleanedPositions = new List<Position>();
        private RoomData? _roomData;


        public CleaningRobot(string inputJson, string outputJson)
        {
            this.InputJson = inputJson;
            this.OutputJson = outputJson;
        }

        public void Start()
        {
            try
            {
                _roomData = Deserialize(InputJson);

                if (_roomData == null)
                    throw new NullReferenceException("Parameter roomData after Deserialize is null");

                //Initialize of parameters

                int startPositionX = _roomData.Start.X;
                int startPositionY = _roomData.Start.Y;
                int battery = _roomData.Battery;

                Console.WriteLine($"Initial Battery Level: {battery}");

                int rows = _roomData.Map.GetLength(0);
                int cols = _roomData.Map.GetLength(1);

                Console.WriteLine("\nMapped Room:");
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Console.Write($"{_roomData.Map[i, j]} ");
                    }
                    Console.WriteLine();
                }

                Position currentPosition = new() { X = _roomData.Start.X, Y = _roomData.Start.Y, Facing = _roomData.Start.Facing };

                foreach (string command in _roomData.Commands)
                {
                    if (_roomData.Battery <= 0)
                    {
                        Console.WriteLine("Battery is depleted. Stopping the robot.");
                        break;
                    }

                    switch (command)
                    {
                        case "TL":
                            battery -= 1;
                            currentPosition.Facing = TurnLeft(currentPosition.Facing);
                            break;
                        case "TR":
                            battery -= 1;
                            currentPosition.Facing = TurnRight(currentPosition.Facing);
                            break;
                        case "A":
                            battery -= 2;
                            if (!MoveForward(ref currentPosition, rows, cols, _roomData.Map))
                            {
                                bool backOffSuccess = false;
                                if (BackOff(ref currentPosition, rows, cols, _roomData.Map, ref battery))
                                {
                                    backOffSuccess = true;
                                    Console.WriteLine("Back off successful. Continuing with the next command.");
                                }

                                if (!backOffSuccess)
                                {
                                    Console.WriteLine("Robot is stuck. Stopping the robot.");
                                    break;
                                }
                            }
                            break;
                        case "B":
                            battery -= 3;
                            if (!MoveBack(ref currentPosition, rows, cols, _roomData.Map))
                            {
                                bool backOffSuccess = false;
                                if (BackOff(ref currentPosition, rows, cols, _roomData.Map, ref battery))
                                {
                                    backOffSuccess = true;
                                    Console.WriteLine("Back off successful. Continuing with the next command.");
                                }

                                if (!backOffSuccess)
                                {
                                    Console.WriteLine("Robot is stuck. Stopping the robot.");
                                    break;
                                }
                            }
                            break;
                        case "C":
                            battery -= 5;
                            _roomData.Map[currentPosition.X, currentPosition.Y] = "S";

                            _cleanedPositions.Add(new Position { X = currentPosition.X, Y = currentPosition.Y });

                            break;
                        default:
                            Console.WriteLine($"Invalid command: {command}");
                            break;
                    }

                    _visitedPositions.Add(new Position { X = currentPosition.X, Y = currentPosition.Y });

                    LogCommand(command);
                }

                HashSet<(int X, int Y)> cleanedSet = new HashSet<(int X, int Y)>();
                foreach (var position in _cleanedPositions)
                {
                    cleanedSet.Add((position.X, position.Y));
                }

                HashSet<(int X, int Y)> visitedSet = new HashSet<(int X, int Y)>();
                foreach (var position in _visitedPositions)
                {
                    visitedSet.Add((position.X, position.Y));
                }

                FinalState finalState = new()
                {
                    Visited = ToList(visitedSet),
                    Cleaned = ToList(cleanedSet),
                    FinalPosition = currentPosition,
                    Battery = battery
                };

                WriteToOutputJson(finalState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} | ERROR | {ex.Message}");
            }
        }

        #region Movement
        private static bool BackOff(ref Position currentPosition, int rows, int cols, string[,] map, ref int battery)
        {
            string[][] backOffSequences =
            {
                new string[] { "TR", "A", "TL" },
                new string[] { "TR", "A", "TR" },
                new string[] { "TR", "A", "TR" },
                new string[] { "TR", "B", "TR", "A" },
                new string[] { "TL", "TL", "A" }
            };

            foreach (string[] sequence in backOffSequences)
            {
                Position backupPosition = new Position { X = currentPosition.X, Y = currentPosition.Y, Facing = currentPosition.Facing };

                foreach (string command in sequence)
                {
                    switch (command)
                    {
                        case "TR":
                            battery -= 1;
                            currentPosition.Facing = TurnRight(currentPosition.Facing);
                            break;
                        case "TL":
                            battery -= 1;
                            currentPosition.Facing = TurnLeft(currentPosition.Facing);
                            break;
                        case "A":
                            battery -= 2;
                            if (!MoveForward(ref currentPosition, rows, cols, map))
                            {
                                currentPosition = backupPosition;
                                LogBackOff(command);
                                break;
                            }
                            break;
                        case "B":
                            battery -= 3;

                            if (!MoveBack(ref currentPosition, rows, cols, map))
                            {
                                currentPosition = backupPosition;
                                LogBackOff(command);
                                break;
                            }
                            break;
                    }
                }

                if (battery > 0)
                    return true;
            }

            return false;
        }

        private static string TurnLeft(string currentFacing)
        {
            switch (currentFacing)
            {
                case "N":
                    return "W";
                case "W":
                    return "S";
                case "S":
                    return "E";
                case "E":
                    return "N";
                default:
                    return currentFacing;
            }
        }

        private static string TurnRight(string currentFacing)
        {
            switch (currentFacing)
            {
                case "N":
                    return "E";
                case "E":
                    return "S";
                case "S":
                    return "W";
                case "W":
                    return "N";
                default:
                    return currentFacing;
            }
        }

        private static bool MoveForward(ref Position currentPosition, int rows, int cols, string[,] map)
        {
            try
            {
                switch (currentPosition.Facing)
                {
                    case "N":
                        if (currentPosition.Y - 1 >= 0 && map[currentPosition.X, currentPosition.Y + 1] != "C"
                            && map[currentPosition.X, currentPosition.Y + 1] != "null")
                        {
                            currentPosition.Y++;
                            return true;
                        }
                        break;
                    case "W":
                        if (currentPosition.X - 1 >= 0 && map[currentPosition.X - 1, currentPosition.Y] != "C"
                            && map[currentPosition.X - 1, currentPosition.Y] != "null")
                        {
                            currentPosition.X--;
                            return true;
                        }
                        break;
                    case "S":
                        if (currentPosition.Y - 1 < rows && map[currentPosition.X, currentPosition.Y - 1] != "C"
                            && map[currentPosition.X - 1, currentPosition.Y] != "null")
                        {
                            currentPosition.Y--;
                            return true;
                        }
                        break;
                    case "E":
                        if (currentPosition.X + 1 < cols && map[currentPosition.X + 1, currentPosition.Y] != "C"
                            && map[currentPosition.X + 1, currentPosition.Y] != "null")
                        {
                            currentPosition.X++;
                            return true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} | ERROR | {ex.Message}");
            }

            return false;
        }

        private static bool MoveBack(ref Position currentPosition, int rows, int cols, string[,] map)
        {
            try
            {
                switch (currentPosition.Facing)
                {
                    case "N":
                        if (currentPosition.X + 1 < rows && map[currentPosition.X + 1, currentPosition.Y] != "C")
                        {
                            currentPosition.X++;
                            return true;
                        }
                        break;
                    case "W":
                        if (currentPosition.Y + 1 < cols && map[currentPosition.X, currentPosition.Y + 1] != "C")
                        {
                            currentPosition.Y++;
                            return true;
                        }
                        break;
                    case "S":
                        if (currentPosition.X - 1 >= 0 && map[currentPosition.X - 1, currentPosition.Y] != "C")
                        {
                            currentPosition.X--;
                            return true;
                        }
                        break;
                    case "E":
                        if (currentPosition.Y - 1 >= 0 && map[currentPosition.X, currentPosition.Y - 1] != "C")
                        {
                            currentPosition.Y--;
                            return true;
                        }
                        break;
                }

                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} | ERROR | {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Helpers
        private static List<Dictionary<string, int>> ToList(HashSet<(int X, int Y)> set)
        {
            var list = new List<Dictionary<string, int>>();
            foreach (var item in set)
            {
                list.Add(new Dictionary<string, int>
                {
                    { "X", item.X },
                    { "Y", item.Y }
                });
            }
            return list;
        }

        private RoomData? Deserialize(string inputJson)
        {
            try
            {
                string json = File.ReadAllText(inputJson);

                var result = JsonConvert.DeserializeObject<RoomData>(json);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} | ERROR | Error reading the JSON file: {ex.Message}");
                return null;
            }
        }

        private void WriteToOutputJson(FinalState finalState)
        {
            try
            {
                string finalStateJson = JsonConvert.SerializeObject(finalState, Formatting.Indented);

                File.WriteAllText(OutputJson, finalStateJson);

                Console.WriteLine($"Robot result written to: {OutputJson}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} | ERROR | {ex.Message}");
            }
        }

        private void LogCommand(string command)
        {
            Console.WriteLine($"Executing command: {command}");
        }

        private static void LogBackOff(string backOffStrategy)
        {
            Console.WriteLine($"Back off strategy triggered: {backOffStrategy}");
        }

        #endregion
    }
}
