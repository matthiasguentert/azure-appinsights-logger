using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class SensitiveDataFilter : ISensitiveDataFilter
    {
        private const string SensitiveValueMask = "***MASKED***";

        private readonly HashSet<string> _sensitiveDataPropertyKeys;
        private readonly IEnumerable<string> _regexesForSensitiveValues;


        public SensitiveDataFilter(BodyLoggerOptions options) : this(options.PropertyNamesWithSensitiveData, options.SensitiveDataRegexes)
        {

        }

        public SensitiveDataFilter(IEnumerable<string> sensitiveDataPropertyKeys, IEnumerable<string> regexesForSensitiveValues)
        {
            _sensitiveDataPropertyKeys = sensitiveDataPropertyKeys.Select(t => t.ToLowerInvariant()).ToHashSet();
            _regexesForSensitiveValues = regexesForSensitiveValues;
        }

        public string RemoveSensitiveData(string textOrJson)
        {
            try
            {
                var json = JsonNode.Parse(textOrJson);
                if (json == null) return string.Empty;

                if (json is JsonValue jValue && TestIfContainsSensitiveData("", jValue.ToString(), _sensitiveDataPropertyKeys, _regexesForSensitiveValues))
                {
                    return SensitiveValueMask;
                }
                RemoveIds(json);
                return json.ToJsonString();
            }
            catch (JsonException)
            {
                if (TestIfContainsSensitiveData("", textOrJson, _sensitiveDataPropertyKeys, _regexesForSensitiveValues))
                {
                    return SensitiveValueMask;
                }
                return textOrJson;
            }
        }

        private void RemoveIds(JsonNode node)
        {
            if (node is JsonObject jObject)
            {
                RemoveIds(jObject);
            }
            else if (node is JsonArray jArray)
            {
                RemoveFromArray(jArray);
            }
        }

        private void RemoveIds(JsonObject? jObject)
        {
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));

            foreach (var jProperty in jObject.ToList())
            {
                if (jProperty.Value is JsonArray array)
                {
                    RemoveFromArray(array);
                }
                else if (jProperty.Value is JsonObject obj)
                {
                    RemoveIds(obj);
                }
                else if (jProperty.Value is JsonValue val
                    && TestIfContainsSensitiveData(jProperty.Key, val.ToString(), _sensitiveDataPropertyKeys, _regexesForSensitiveValues))
                {
                    jObject[jProperty.Key] = SensitiveValueMask;
                }
            }
        }

        private void RemoveFromArray(JsonArray jArray)
        {
            if (jArray == null) throw new ArgumentNullException(nameof(jArray));

            foreach (var jNode in jArray.Where(v => v != null))
            {
                RemoveIds(jNode);
            }
        }

        private bool TestIfContainsSensitiveData(
            string propertyName,
            string propertyValue,
            HashSet<string> sensitiveDataPropertyKeys,
            IEnumerable<string> regexesForSensitiveValues
            )
        {
            var propertyNameToCompare = propertyName.ToLowerInvariant();
            var sensitivePropertyName = sensitiveDataPropertyKeys.Contains(propertyNameToCompare)
                || sensitiveDataPropertyKeys.Any(s => propertyNameToCompare.Contains(s));

            if (sensitivePropertyName)
            {
                return true;
            }

            foreach (var regex in regexesForSensitiveValues)
            {
                if (Regex.IsMatch(propertyValue, regex))
                {
                    return true;
                }
            }


            return false;
        }
    }
}
