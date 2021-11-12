# Extended HTTP request & response logging with Application Insights

## Introduction 

This nuget package provides a custom middleware that allows to write the body of an HTTP request/response to a custom dimension. 

![](https://i.imgur.com/0fxsnKN.png)

## Features

- Log request & response body to Application Insights
- Configure maximum length to store
- Provide optional cut off text
- Configure name of custom dimension key
- Filter based on request path and content type 

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

    services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
    services.AddRequestLogging();
    services.AddResponseLogging();

    // ...
}
```

Finally configure the request pipeline. 

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        
        app.UseRequestLogging();
        app.UseResponseLogging();
    }
    
    // ...
}
```
## Configuration 

You can overwrite the default settings as follows:

```csharp
services.AddRequestLogging(o =>
{
    o.MaxBytes = 10000;
    o.CutOffText = "SNIP";
    // ...
});
```

Or stick with the defaults which are defined in `RequestLoggerOptions` and `ResponseLoggerOptions`.

### RequestLoggerOptions

```csharp
public class RequestLoggerOptions
{
    public string PropertyKey { get; set; } = "RequestBody";

    public string[] HttpVerbs { get; set; } = { "POST", "PUT" };

    public int MaxBytes { get; set; } = 80000;

    public string CutOffText { get; set; } = "\n---8<------------------------\nSSHORTENED-DUE-TO-MAXSIZE-LIMIT";

    public string? ContentType { get; set; } = null;

    public string Path { get; set; } = "/";
}
``` 

### ResonseLoggerOptions

```csharp
public class ResponseLoggerOptions
{
    public string PropertyKey { get; set; } = "ResponseBody";

    public int MaxBytes { get; set; } = 80000;

    public string CutOffText { get; set; } = "\n---8<------------------------\nSSHORTENED-DUE-TO-MAXSIZE-LIMIT";

    public string? ContentType { get; set; } = MediaTypeNames.Application.Json;
}
```
