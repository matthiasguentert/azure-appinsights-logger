using Azureblue.ApplicationInsights.RequestLogging;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace ApplicationInsightsRequestLoggingTests.Filters
{
    public class SensitiveDataFilterTests
    {
        [Fact]
        public void Should_mask_tokens_with_sensitive_data_from_json()
        {
            // Arrange
            var bodyLoggerOptions = new BodyLoggerOptions
            {
                PropertyNamesWithSensitiveData = new List<string>() { "token" },
                SensitiveDataRegexes = new List<string>()
            };

            var jsonWithToken = JsonSerializer.Serialize(new { token = "some-super-secret-token" });
            var filter = new SensitiveDataFilter(Options.Create(bodyLoggerOptions));

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
            var bodyLoggerOptions = new BodyLoggerOptions
            {
                PropertyNamesWithSensitiveData = new List<string>() { "password" },
                SensitiveDataRegexes = new List<string>()
            };

            var jsonWithToken = JsonSerializer.Serialize(new { someObject = new { password = "some-super-secret-token" } });
            var filter = new SensitiveDataFilter(Options.Create(bodyLoggerOptions));

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
            var bodyLoggerOptions = new BodyLoggerOptions
            {
                PropertyNamesWithSensitiveData = new List<string>() { "password" },
                SensitiveDataRegexes = new List<string>()
            };

            var jsonWithToken = JsonSerializer.Serialize(new { someObject = new { userPassword = "some-super-secret-token" } });
            var filter = new SensitiveDataFilter(Options.Create(bodyLoggerOptions));

            // Act
            var result = filter.RemoveSensitiveData(jsonWithToken);

            // Assert
            var jsonStrippedFromSensitiveData = JsonSerializer.Serialize(new { someObject = new { userPassword = "***MASKED***" } });
            result.Should().Be(jsonStrippedFromSensitiveData);
        }

        private const string CreditCardRegex = @"(?:4[0-9]{12}(?:[0-9]{3})?|[25][1-7][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})";
        private const string SampleCreditCardNumber = "4012888888881881";

        [Fact]
        public void Should_mask_values_based_on_regex()
        {
            // Arrange
            var bodyLoggerOptions = new BodyLoggerOptions
            {
                PropertyNamesWithSensitiveData = new List<string>() { "password" },
                SensitiveDataRegexes = new List<string> { CreditCardRegex }
            };

            var jsonWithToken = JsonSerializer.Serialize(new { someObject = new { randomTokenName = SampleCreditCardNumber } });
            var filter = new SensitiveDataFilter(Options.Create(bodyLoggerOptions));

            // Act
            var result = filter.RemoveSensitiveData(jsonWithToken);

            // Assert
            var jsonStrippedFromSensitiveData = JsonSerializer.Serialize(new { someObject = new { randomTokenName = "***MASKED***" } });
            result.Should().Be(jsonStrippedFromSensitiveData);
        }

        [Fact]
        public void SensitiveDataFilter_should_remove_creditcard_number_from_plain_text_if_configured()
        {
            // Arrange
            var bodyLoggerOptions = new BodyLoggerOptions
            {
                PropertyNamesWithSensitiveData = new List<string>() { "token" },
                SensitiveDataRegexes = new List<string> { CreditCardRegex }
            };

            var plainTextWithToken = $"token: some-not-so-{SampleCreditCardNumber}secret-token-but-with-cc-inside";
            var filter = new SensitiveDataFilter(Options.Create(bodyLoggerOptions));

            // Act
            var result = filter.RemoveSensitiveData(plainTextWithToken);

            // Assert
            result.Should().Be("***MASKED***");
        }
    }
}
