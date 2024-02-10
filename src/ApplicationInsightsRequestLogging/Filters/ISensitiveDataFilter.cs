namespace Azureblue.ApplicationInsights.RequestLogging
{
    public interface ISensitiveDataFilter
    {
        string RemoveSensitiveData(string textOrJson);
    }
}
