namespace Azureblue.ApplicationInsights.RequestLogging.Filters
{
    public interface ISensitiveDataFilter
    {
        string RemoveSensitiveData(string textOrJson);
    }
}
