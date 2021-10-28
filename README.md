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

## Installation 

Just pull in the nuget package like so: 

```
dotnet add package Azureblue.ApplicationInsights.RequestLogging
```

Then you'll have to register the middleware in your `Startup` class with your container. 

```
using Azureblue.ApplicationInsights.RequestLogging;

// ...

public void ConfigureServices(IServiceCollection services)
{
    // ...

    services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
    services.AddRequestLogging(new RequestLoggerOptions
    {
        PropertyKey = "RequestBody",
        HttpVerbs = new[] { "POST" },
        MaxSize = 100,
        CutOffText = "SNIP",
        ContentType = "application/json",
        Path = "/"
    });
    services.AddResponseLogging(new ResponseLoggerOptions
    {
        PropertyKey = "ResponseBody",
        MaxSize = 100,
        CutOffText = "SNIP",
        ContentType = "application/json",
    });

    // ...
}
```

Finally configure the request pipeline. 

```
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

> A word of warning! Writing the content of an HTTP body to Application Insights might reveal sensitive user information that otherwise would be hidden and protected in transfer via TLS. So use this with care and only during debugging or developing!

## Configuration 

Use an instance of `RequestLoggerOptions` and `ResponseLoggerOptions` to configure the middleware. See the default values below. 

### RequestLoggerOptions

```
public class RequestLoggerOptions
{
    public string PropertyKey { get; set; } = "RequestBody";

    public string[] HttpVerbs { get; set; } = { "POST", "PUT" };

    public int MaxSize { get; set; } = 100;

    public string CutOffText { get; set; } = "\n---8<------------------------\nSSHORTENED-DUE-TO-MAXSIZE-LIMIT";

    public string? ContentType { get; set; } = null;

    public string Path { get; set; } = "/";
}
``` 

### ResonseLoggerOptions

```
public class ResponseLoggerOptions
{
    public string PropertyKey { get; set; } = "ResponseBody";

    public int MaxSize { get; set; } = 100;

    public string CutOffText { get; set; } = "\n---8<------------------------\nSSHORTENED-DUE-TO-MAXSIZE-LIMIT";

    public string? ContentType { get; set; } = MediaTypeNames.Application.Json;
}
```
