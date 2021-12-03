namespace Azureblue.ApplicationInsights.RequestLogging.SensitiveDataFilter
{
    public interface ISensitiveDataFilter
    {
        string RemoveSensitiveData(string textOrJson);
    }
}
