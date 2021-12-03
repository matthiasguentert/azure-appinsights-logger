using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Azureblue.ApplicationInsights.RequestLogging.SensitiveDataFilter
{
    public class SensitiveDataFilter
    {
        public HashSet<string> _sensitiveDataPropertyKeys;

        public SensitiveDataFilter(IEnumerable<string> sensitiveDataPropertyKeys)
        {
            _sensitiveDataPropertyKeys = sensitiveDataPropertyKeys.Select(t => t.ToLowerInvariant()).ToHashSet();
        }

        public string RemoveSensitiveData(string textOrJson)
        {
            try
            {
                var json = JsonNode.Parse(textOrJson);
                if (json == null) return string.Empty;
                RemoveIds(json, _sensitiveDataPropertyKeys);
                return json.ToJsonString();
            }
            catch (JsonException)
            {
                return textOrJson;
            }
        }
        private void RemoveIds(JsonNode node, HashSet<string> sensitiveDataPropertyKeys)
        {
            if (node is JsonObject jObject)
            {
                RemoveIds(jObject, sensitiveDataPropertyKeys);
            }
            else if (node is JsonArray jArray)
            {
                RemoveFromArray(jArray, sensitiveDataPropertyKeys);
            }
        }

        private void RemoveIds(JsonObject? jObject, HashSet<string> sensitiveDataPropertyKeys)
        {
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));

            foreach (var jProperty in jObject.ToList())
            {
                if (jProperty.Value is JsonArray array)
                {
                    RemoveFromArray(array, sensitiveDataPropertyKeys);
                }
                else if (jProperty.Value is JsonObject obj)
                {
                    RemoveIds(obj, sensitiveDataPropertyKeys);
                }
                else if (jProperty.Value is JsonValue val
                    && TestIfContainsSensitiveData(jProperty.Key, val.ToString(), sensitiveDataPropertyKeys))
                {
                    jObject[jProperty.Key] = "***MASKED***";
                }
            }
        }

        private void RemoveFromArray(JsonArray jArray, HashSet<string> props)
        {
            if (jArray == null) throw new ArgumentNullException(nameof(jArray));

            foreach (var jNode in jArray.Where(v => v != null))
            {
                RemoveIds(jNode, props);
            }
        }

        private bool TestIfContainsSensitiveData(string propertyName, string propertyValue, HashSet<string> sensitiveDataPropertyKeys)
        {
            var propertyNameToCompare = propertyName.ToLowerInvariant();
            return sensitiveDataPropertyKeys.Contains(propertyNameToCompare)
                || sensitiveDataPropertyKeys.Any(s => propertyNameToCompare.Contains(s));
        }
    }
}
