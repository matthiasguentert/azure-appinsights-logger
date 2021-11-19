using Azureblue.ApplicationInsights.RequestLogging;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
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

        [Fact]
        public async Task Should_Read_Full_Request()
        {
            // Arrange
            var reader = new BodyReader();
            var requestBody = "Hello from test environment";
            var context = HttpContextTestFactory.CreateWithRequestBody(requestBody);

            // Act
            var result = await reader.ReadRequestBodyAsync(context, requestBody.Length, string.Empty);

            // Assert
            result.Should().StartWith("Hello from test environment");
        }

        [Fact]
        public void Should_Throw_If_PrepareResponseBodyReading_Has_Not_Been_Called()
        {
            // Arrange
            var reader = new BodyReader();
            var context = new DefaultHttpContext();

            // Act
            Func<Task> action = async () =>
            {
                var result = await reader.ReadResponseBodyAsync(context, 100, "SNIP");
            };

            // Assert
            action.Should().ThrowAsync<ArgumentNullException>().WithMessage("Call PrepareResponseBodyReading() before passing control to the next delegate!");
        }

        [Fact(Skip = "Doesn't make sense yet!")]
        public async Task Should_Read_Full_Response()
        {
            // Arrange
            var reader = new BodyReader();
            var responseBody = "Hello from test environment";
            var context = HttpContextTestFactory.CreateWithResponseBody(responseBody);

            // Act
            reader.PrepareResponseBodyReading(context);
            var result = await reader.ReadResponseBodyAsync(context, responseBody.Length, string.Empty);

            // Assert
            result.Should().Be("Hello from test environment\r\n");
        }
    }
}