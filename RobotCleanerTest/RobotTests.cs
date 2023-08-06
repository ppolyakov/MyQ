namespace RobotCleanerTest
{
    [TestClass]
    public class RobotTests
    {
        [TestMethod]
        public void TestInvalidInput()
        {
            // Arrange
            string inputJson = "{ \"invalidJson\": \"this is invalid\" }";

            // Act & Assert
            Assert.ThrowsException<JsonSerializationException>(() => JsonConvert.DeserializeObject<RoomData>(inputJson));
        }

        [TestMethod]
        public void TestOutOfBattery()
        {
            // Arrange
            string inputJsonPath = "C:\\MyQ\\tests\\input_1.json";
            string outputJsonPath = "C:\\MyQ\\tests\\output_1.json";
            var robot = new CleaningRobot(inputJsonPath, outputJsonPath);

            // Act
            robot.Start();

            // Assert
            var finalStateJson = File.ReadAllText(outputJsonPath);
            var finalState = JsonConvert.DeserializeObject<FinalState>(finalStateJson);
            Assert.AreEqual(0, finalState.Battery);
        }

        [TestMethod]
        public void TestBackOffSequenceNTriggered()
        {
            // Arrange
            string inputJsonPath = "C:\\MyQ\\tests\\input_2.json";
            string outputJsonPath = "C:\\MyQ\\tests\\output_2.json";
            var robot = new CleaningRobot(inputJsonPath, outputJsonPath);

            // Act
            robot.Start();

            // Assert
            var finalStateJson = File.ReadAllText(outputJsonPath);
            var finalState = JsonConvert.DeserializeObject<FinalState>(finalStateJson);
            Assert.AreEqual(1, finalState.Battery);
        }

        [TestMethod]
        public void TestRobotStuck()
        {
            // Arrange
            string inputJsonPath = "C:\\MyQ\\tests\\input_3.json";
            string outputJsonPath = "C:\\MyQ\\tests\\output_3.json";
            var robot = new CleaningRobot(inputJsonPath, outputJsonPath);

            // Act
            robot.Start();

            // Assert
            var finalStateJson = File.ReadAllText(outputJsonPath);
            var finalState = JsonConvert.DeserializeObject<FinalState>(finalStateJson);
            Assert.IsFalse(finalState.Visited.Count > 1);
        }
    }
}