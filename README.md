# Extended request logging with Application Insights

## Introduction 

This nuget package provides a custom middleware that allows to write the body of an HTTP request/response to a custom dimension. 

![](https://i.imgur.com/CNbVKsx.png)

## Features

- Log request & response body to Application Insights
- Configure HTTP verbs that will trigger logging 
- Configure HTTP status code ranges that will trigger logging
- Configure maximum body length to store
- Provide optional cut off text
- Configure name of custom dimension keys
- Disable IP masking without the need to modify the App Insights resource as described [here](https://learn.microsoft.com/en-us/azure/azure-monitor/app/ip-collection?tabs=net)

> A word of warning! Writing the content of an HTTP body to Application Insights might reveal sensitive user information that otherwise would be hidden and protected in transfer via TLS. So use this with care and only during debugging or developing!

## Installation 

Just pull in the nuget package like so: 

```
dotnet add package Azureblue.ApplicationInsights.RequestLogging
```

Then you'll have to register the middleware in your `Startup` class with your container. 

```csharp
using Azureblue.ApplicationInsights.RequestLogging;

// ...

public void ConfigureServices(IServiceCollection services)
{
    // ...

    // Register App Insights 
    services.AddApplicationInsightsTelemetry();
    
    // Register this middleware
    services.AddAppInsightsHttpBodyLogging();

    // ...
}
```

Finally configure the request pipeline. Make sure the call to `UseAppInsightsHttpBodyLogging` happens as early as possible as the [order matters](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0#middleware-order). Have a look at this [issue](https://github.com/matthiasguentert/azure-appinsights-logger/issues/11)

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseAppInsightsHttpBodyLogging();
    }
    
    // ...
}
```
## Configuration 

You can overwrite the default settings as follows...

```csharp
services.AddAppInsightsHttpBodyLogging(o => {
    o.HttpCodes.Add(StatusCodes.Status200OK);
    o.HttpVerbs.Add(HttpMethods.Get);
    o.MaxBytes = 10000;
    o.Appendix = "\nSNIP";
    o.DisableIpMasking = true;
});
```

...or stick with the defaults which are defined in `BodyLoggerOptions`.

### BodyLoggerOptions

```csharp
public class BodyLoggerOptions
{
    public BodyLoggerOptions()
    {
        HttpCodes.AddRange(StatusCodeRanges.Status4xx);
        HttpCodes.AddRange(StatusCodeRanges.Status5xx);
    }

    /// <summary>
    ///     Only write to App Insights on these HTTP status codes
    /// </summary>
    public List<int> HttpCodes { get; set; } = new List<int>();

    /// <summary>
    ///     Only these HTTP verbs will trigger logging
    /// </summary>
    public List<string> HttpVerbs { get; set; } = new List<string>()
    {
        HttpMethods.Post, 
        HttpMethods.Put,
        HttpMethods.Patch
    };

    /// <summary>
    ///     Which property key should be used
    /// </summary>
    public string RequestBodyPropertyKey { get; set; } = "RequestBody";

    /// <summary>
    ///     Which property key should be used
    /// </summary>
    public string ResponseBodyPropertyKey { get; set; } = "ResponseBody";

    /// <summary>
    ///     Which property key should be used
    /// </summary>
    public string ClientIpPropertyKey { get; set; } = "ClientIp";
    
    /// <summary>
    ///     Defines the amount of bytes that should be read from HTTP context
    /// </summary>
    public int MaxBytes { get; set; } = 80000;

    /// <summary>
    ///     Defines the text to append in case the body should be truncated <seealso cref="MaxBytes"/>
    /// </summary>
    public string Appendix { get; set; } = "\n---8<------------------------\nTRUNCATED DUE TO MAXBYTES LIMIT";

    /// <summary>
    ///     Controls storage of client IP addresses https://learn.microsoft.com/en-us/azure/azure-monitor/app/ip-collection?tabs=net
    /// </summary>
    public bool DisableIpMasking { get; set; } = false;
}
```
