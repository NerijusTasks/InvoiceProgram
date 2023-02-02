using InvoiceProgram;

namespace Tests
{
    public class GetAnswerTests
    {
        [Theory]
        [InlineData("Y", true)]
        [InlineData("N", false)]
        public void GetAnswerShouldReturnExpectedResult(string input, bool expectedResult)
        {
            // Arrange
            ApplicationService service = new ApplicationService();
            var inputStringReader = new StringReader(input);
            Console.SetIn(inputStringReader);

            // Act
            var result = service.GetAnswer("Test message");

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("5", 5)]
        [InlineData("10,5", 10.5)]
        public void ReadNumericShouldReturnExpectedResult(string input, decimal expectedResult)
        {
            // Arrange
            ApplicationService service = new ApplicationService();
            var inputStringReader = new StringReader(input);
            Console.SetIn(inputStringReader);

            // Act
            var result = service.ReadNumericValue("Test Message");

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}


