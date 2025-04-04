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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInsightsRequestLoggingTests
{
    public class BodyLoggerMiddlewareTests
    {
        [Fact]
        public void BodyLoggerMiddleware_should_throw_if_ctor_params_null()
        {
            // Arrange & Act
            var action = () => { var middleware = new BodyLoggerMiddleware(null, null, null, null); };

            // Assert
            action.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public async Task BodyLoggerMiddleware_should_not_log_request_body_if_downstream_exception_and_disabled()
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
                            services.AddOptions<BodyLoggerOptions>().Configure(options =>
                            {
                                options.EnableBodyLoggingOnExceptions = false;
                            });
                            services.AddTransient<IBodyReader, BodyReader>();
                            services.AddTransient<ISensitiveDataFilter, SensitiveDataFilter>();
                            services.AddSingleton(telemetryWriter.Object);
                            services.AddTransient<BodyLoggerMiddleware>();
                        })
                        .Configure(app =>
                        {
                            app.UseMiddleware<BodyLoggerMiddleware>();
                            app.Run(_ => throw new Exception("downstream exception"));
                        });
                })
                .StartAsync();

            // Act
            try
            {
                await host.GetTestClient().PostAsync("/", new StringContent("Hello from client"));
            }
            catch (Exception)
            {
                //ignore errors thrown by test client
            }

            // Assert
            telemetryWriter.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task BodyLoggerMiddleware_should_log_request_body_if_downstream_exception_and_enabled()
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
                            services.AddOptions<BodyLoggerOptions>().Configure(options =>
                            {
                                options.EnableBodyLoggingOnExceptions = true;
                            });
                            services.AddTransient<IBodyReader, BodyReader>();
                            services.AddTransient<ISensitiveDataFilter, SensitiveDataFilter>();
                            services.AddSingleton(telemetryWriter.Object);
                            services.AddTransient<BodyLoggerMiddleware>();
                        })
                        .Configure(app =>
                        {
                            app.UseMiddleware<BodyLoggerMiddleware>();
                            app.Run(_ => throw new Exception("downstream exception"));
                        });
                })
                .StartAsync();

            // Act
            try
            {
                await host.GetTestClient().PostAsync("/", new StringContent("Hello from client"));
            }
            catch (Exception)
            {
                //ignore errors thrown by test client
            }

            // Assert
            telemetryWriter.Verify(x => x.Write(It.IsAny<HttpContext>(), "RequestBody", "Hello from client"), Times.Once);
        }

        [Fact]
        public async Task BodyLoggerMiddleware_should_send_data_to_AppInsights()
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
        ///     This used to blow up with the following message and return a 500 error
        ///     <c>System.InvalidOperationException: Response Content-Length mismatch: too few bytes written (0 of XXX)</c>
        ///     because the original stream was never returned.
        /// </summary>
        [Fact]
        public async Task BodyLoggerMiddleware_should_not_send_data_to_AppInsights_when_status_code_is_not_of_interest()
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
        public async Task BodyLoggerMiddleware_should_leave_body_intact()
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
        public async Task BodyLoggerMiddleware_should_redact_password()
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
                            services.AddOptions<BodyLoggerOptions>().Configure(options =>
                            {
                                options.PropertyNamesWithSensitiveData = new List<string> { "password" };
                                options.SensitiveDataRegexes = new List<string>();
                            });
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
            var response = await host.GetTestClient().PostAsync("/", new StringContent("{\"email\":\"fred@mayekawa.com\",\"password\":\"P@ssw0rd!\"}"));

            // Assert
            telemetryWriter.Verify(x => x.Write(It.IsAny<HttpContext>(), "RequestBody", "{\"email\":\"fred@mayekawa.com\",\"password\":\"***MASKED***\"}"), Times.Once);
            telemetryWriter.Verify(x => x.Write(It.IsAny<HttpContext>(), "ResponseBody", "{\"email\":\"fred@mayekawa.com\",\"password\":\"***MASKED***\"}"), Times.Once);
        }

        [Fact]
        public async Task BodyLoggerMiddleware_should_properly_pass()
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
        public async Task BodyLoggerMiddleware_should_disable_ip_masking()
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

		[Fact]
		public async Task BodyLoggerMiddleware_should_not_log_request_body_if_excluded_content_type()
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
							services.AddOptions<BodyLoggerOptions>().Configure(options => 
                            {
                                options.ExcludedContentTypes = new List<string> { "multipart/form-data" };
							});
							services.AddTransient<IBodyReader, BodyReader>();
							services.AddTransient<ISensitiveDataFilter, SensitiveDataFilter>();
							services.AddSingleton(telemetryWriter.Object);
							services.AddTransient<BodyLoggerMiddleware>();
						})
						.Configure(app =>
                        {
							app.UseMiddleware<BodyLoggerMiddleware>();
						});
				})
				.StartAsync();

			// Act
			try 
            {
				await host.GetTestClient().PostAsync("/", new StringContent("Hello from client", encoding: null, mediaType: "multipart/form-data"));
			}
            catch(Exception)
            {
				//ignore errors thrown by test client
			}

			// Assert
			telemetryWriter.VerifyNoOtherCalls();
		}
	}
}
