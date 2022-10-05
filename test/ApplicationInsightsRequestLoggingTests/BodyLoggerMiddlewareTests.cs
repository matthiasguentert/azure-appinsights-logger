using Azureblue.ApplicationInsights.RequestLogging;
using FluentAssertions;
using System;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationInsightsRequestLoggingTests
{
    public class BodyLoggerMiddlewareTests
    {
        [Fact]
        public void BodyLoggerMiddleware_Should_Throw_If_Ctor_Params_Null()
        {
            // Arrange & Act
            Action action = () => { var middleware = new BodyLoggerMiddleware(null, null, null); };

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async void BodyLoggerMiddleware_Should_Send_Data_To_AppInsights()
        {
            // Arrange
            var telemetryWriter = new Mock<ITelemetryWriter>();

            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddTransient<IBodyReader, BodyReader>();
                            services.AddSingleton(telemetryWriter.Object);
                            services.AddTransient<BodyLoggerMiddleware>();
                        })
                        .Configure(app =>
                        {
                            app.UseMiddleware<BodyLoggerMiddleware>();
                            app.Run(async context =>
                            {
                                // Send request body back in response body
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Request.Body.CopyToAsync(context.Response.Body);
                            });
                        });
                })
                .StartAsync();

            // Act
            var response = await host.GetTestClient().PostAsync("/", new StringContent("Hello from client"));

            // Assert
            telemetryWriter.Verify(x => x.Write(It.IsAny<HttpContext>(), "RequestBody", "Hello from client"), Times.Once);
            telemetryWriter.Verify(x => x.Write(It.IsAny<HttpContext>(), "ResponseBody", "Hello from client"), Times.Once);
        }
        
        /// <summary>
        /// This used to blow up with the following message and return a 500 error
        /// <c>System.InvalidOperationException: Response Content-Length mismatch: too few bytes written (0 of XXX)</c>
        /// because the original stream was never returned.
        /// </summary>
        [Fact]
        public async void BodyLoggerMiddleware_Should_Not_Send_Data_To_AppInsights_When_StatusCode_Is_Not_Of_Interest()
        {
            // Arrange
            var telemetryWriter = new Mock<ITelemetryWriter>();

            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddTransient<IBodyReader, BodyReader>();
                            services.AddSingleton(telemetryWriter.Object);
                            services.AddTransient<BodyLoggerMiddleware>();
                        })
                        .Configure(app =>
                        {
                            app.UseMiddleware<BodyLoggerMiddleware>();
                            app.Run(async context =>
                            {
                                // Send request body back in response body
                                context.Response.StatusCode = StatusCodes.Status200OK;
                                await context.Request.Body.CopyToAsync(context.Response.Body);
                            });
                        });
                })
                .StartAsync();

            // Act
            var response = await host.GetTestClient().PostAsync("/", new StringContent("Hello from client"));

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Be("Hello from client");
            
            telemetryWriter.Verify(x => x.Write(It.IsAny<HttpContext>(), "RequestBody", "Hello from client"), Times.Never);
            telemetryWriter.Verify(x => x.Write(It.IsAny<HttpContext>(), "ResponseBody", "Hello from client"), Times.Never);
        }

        [Fact]
        public async void BodyLoggerMiddleware_Should_Leave_Body_intact()
        {
            // Arrange            
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddAppInsightsHttpBodyLogging(o =>
                            {
                                // Ensure middleware kicks in on success status
                                o.HttpCodes.Add(StatusCodes.Status200OK);
                            });
                        })
                        .Configure(app =>
                        {
                            app.UseAppInsightsHttpBodyLogging();
                            app.Run(async context =>
                            {
                                // Send request body back in response body
                                await context.Request.Body.CopyToAsync(context.Response.Body);
                            });
                        });
                })
                .StartAsync();

            // Act
            var response = await host.GetTestClient().PostAsync("/", new StringContent("Hello from client"));

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Be("Hello from client");
        }

        [Fact]
        public async void BodyLoggerMiddleware_Should_Properly_Pass()
        {
            // Arrange            
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddAppInsightsHttpBodyLogging();
                        })
                        .Configure(app =>
                        {
                            app.UseAppInsightsHttpBodyLogging();
                            app.Run(async context =>
                            {
                                await context.Response.WriteAsync("Hello from terminating delegate!");
                            });
                        });
                })
                .StartAsync();

            // Act
            var response = await host.GetTestClient().GetAsync("/");

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Be("Hello from terminating delegate!");
        }
        
        [Fact]
        public async void BodyLoggerMiddleware_Should_Disable_Ip_Masking()
        {
            // Arrange            
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddAppInsightsHttpBodyLogging(o =>
                            {
                                // Ensure middleware kicks in on success status
                                o.HttpCodes.Add(StatusCodes.Status200OK);
                                o.DisableIpMasking = true;
                            });
                        })
                        .Configure(app =>
                        {
                            app.UseAppInsightsHttpBodyLogging();
                            app.Run(async context =>
                            {
                                // Send request body back in response body
                                await context.Request.Body.CopyToAsync(context.Response.Body);
                            });
                        });
                })
                .StartAsync();

            // Act
            var response = await host.GetTestClient().PostAsync("/", new StringContent("Hello from client"));

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Be("Hello from client");
        }
    }
}
