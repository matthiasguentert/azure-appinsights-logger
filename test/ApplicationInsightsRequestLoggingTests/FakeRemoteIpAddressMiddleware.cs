using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ApplicationInsightsRequestLoggingTests;

public class FakeRemoteIpAddressMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.168.1.32");
        await next(context);
    }
}
