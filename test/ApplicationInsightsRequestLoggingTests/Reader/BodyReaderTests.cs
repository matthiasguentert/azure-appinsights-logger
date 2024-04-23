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
        public async Task Should_read_request_body_and_truncate()
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
        public async Task Should_read_full_request()
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
        public void Should_throw_if_PrepareResponseBodyReading_has_not_been_called()
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
    }
}
