using Azureblue.ApplicationInsights.RequestLogging.SensitiveDataFilter;
using FluentAssertions;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace ApplicationInsightsRequestLoggingTests.Reader
{
    public class SensitiveDataFilterTests
    {
        [Fact]
        public void Should_mask_tokens_with_sensitive_data_from_json()
        {
            // Arrange
            var jsonWithToken = JsonSerializer.Serialize(new { token = "some-super-secret-token" });
            var filter = new SensitiveDataFilter(new HashSet<string> { "token" });

            // Act
            var result = filter.RemoveSensitiveData(jsonWithToken);

            // Assert
            var jsonStrippedFromSensitiveData = JsonSerializer.Serialize(new { token = "***MASKED***" });
            result.Should().Be(jsonStrippedFromSensitiveData);
        }

        [Fact]
        public void Should_mask_tokens_with_sensitive_data_from_nested_object_json()
        {
            // Arrange
            var jsonWithToken = JsonSerializer.Serialize(new { someObject = new { password = "some-super-secret-token" } });
            var filter = new SensitiveDataFilter(new HashSet<string> { "password" });

            // Act
            var result = filter.RemoveSensitiveData(jsonWithToken);

            // Assert
            var jsonStrippedFromSensitiveData = JsonSerializer.Serialize(new { someObject = new { password = "***MASKED***" } });
            result.Should().Be(jsonStrippedFromSensitiveData);
        }

        [Fact]
        public void Should_mask_tokens_with_sensitive_data_even_if_its_not_equal_match_json()
        {
            // Arrange
            var jsonWithToken = JsonSerializer.Serialize(new { someObject = new { userPassword = "some-super-secret-token" } });
            var filter = new SensitiveDataFilter(new HashSet<string> { "password" });

            // Act
            var result = filter.RemoveSensitiveData(jsonWithToken);

            // Assert
            var jsonStrippedFromSensitiveData = JsonSerializer.Serialize(new { someObject = new { userPassword = "***MASKED***" } });
            result.Should().Be(jsonStrippedFromSensitiveData);
        }

        [Fact]
        public void SensitiveDataFilter_wont_remove_sensitive_data_from_plain_text()
        {
            // Arrange
            var plainTextWithToken = "token: some-super-secret-token";
            var filter = new SensitiveDataFilter(new HashSet<string> { "token" });

            // Act
            var result = filter.RemoveSensitiveData(plainTextWithToken);

            // Assert
            result.Should().Be(plainTextWithToken);
        }
    }
}