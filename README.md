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
- Optionally log request body even in case a downstream middleware or handler throws
- Allows redacting sensitive data like tokens, passwords, credit card numbers, etc.

> A word of warning! Writing the content of an HTTP body to Application Insights might reveal sensitive user information that otherwise would be hidden and protected in transfer via TLS. So use this middleware with care - with great power comes great responsibility (I always wanted to say that...)!

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

Finally configure the request pipeline. Make sure the call to `UseAppInsightsHttpBodyLogging` happens as early as possible as the [order matters](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0#middleware-order). Have a look at this [issue](https://github.com/matthiasguentert/azure-appinsights-logger/issues/11). An example can be found in the `ManualTests` project.

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
    o.DisableIpMasking = true;
    o.EnableBodyLoggingOnExceptions = true;
});
```

...or stick with the defaults which are defined in [`BodyLoggerOptions`](https://github.com/matthiasguentert/azure-appinsights-logger/blob/500185bdb1a73bd74cb9a512ca954e1afc494872/src/ApplicationInsightsRequestLogging/Options/BodyLoggerOptions.cs).

## Acknowledgement

This is an open source project, and I'd like to thank its contributors that added new features to the code! Thank you!🙏🏼
