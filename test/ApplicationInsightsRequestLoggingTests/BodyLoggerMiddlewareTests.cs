using Azureblue.ApplicationInsights.RequestLogging;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
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
            var action = () => { var middleware = new BodyLoggerMiddleware(null, null, null, null); };

            // Assert
            action.Should().Throw<NullReferenceException>();
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
                            services.AddTransient<ISensitiveDataFilter, SensitiveDataFilter>();
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
                            services.AddTransient<ISensitiveDataFilter, SensitiveDataFilter>();
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
                            services.AddTransient<ISensitiveDataFilter, SensitiveDataFilter>();
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
                        .ConfigureTestServices(services =>
                        {
                            // Set fake client IP 
                            services.AddSingleton<FakeRemoteIpAddressMiddleware>();
                            
                            // Register stub telemetry channel for testing
                            services.AddSingleton<ITelemetryChannel, FakeTelemetryChannel>();
                            
                            // Register app insights infrastructure
                            services.AddApplicationInsightsTelemetry();
                            
                            // Add request body logging middleware
                            services.AddAppInsightsHttpBodyLogging(o =>
                            {
                                // Ensure middleware kicks in on success status
                                o.HttpCodes.Add(StatusCodes.Status200OK);
                                o.DisableIpMasking = true;
                            });
                        })
                        .Configure(app =>
                        {
                            app.UseMiddleware<FakeRemoteIpAddressMiddleware>();
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
            _ = await host.GetTestClient().PostAsync("/", new StringContent("Hello from client"));
            
            // Assert
            var channel = host.Services.GetService<ITelemetryChannel>() as FakeTelemetryChannel;
            channel.Should().NotBeNull();
            
            // Unfortunately, threads can't be synchronized in a deterministic manner
            SpinWait.SpinUntil(() =>
            {
                Thread.Sleep(10);
                return channel?.SentTelemtries.Count >= 1;
            }, TimeSpan.FromSeconds(3)).Should().BeTrue();
            
            var requestItem = channel?.SentTelemtries.First() as RequestTelemetry;
            requestItem.Should().NotBeNull();
            requestItem?.Properties["ClientIp"].Should().Be("127.168.1.32");
        }
    }
}
