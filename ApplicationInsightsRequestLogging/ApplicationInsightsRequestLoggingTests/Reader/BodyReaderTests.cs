using Azureblue.ApplicationInsights.RequestLogging;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace ApplicationInsightsRequestLoggingTests.Reader
{
    public class BodyReaderTests
    {
        [Fact]
        public async Task Should_Read_Request_Body_And_Truncate()
        {
            // Arrange
            var reader = new BodyReader();
            var requestBody = "Hello from test environment";
            var context = HttpContextTestFactory.CreateWithRequestBody(requestBody);

            // Act
            var result = await reader.ReadRequestBodyAsync(context, 5, "SNIP");

            // Assert
            result.Should().Be("HelloSNIP");
        }
    }
}