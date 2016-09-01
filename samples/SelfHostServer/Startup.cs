using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Server;

namespace SelfHostServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Server options can be configured here instead of in Main.
            // 
            // services.Configure<WebListenerOptions>(options =>
            // {
            //     options.ListenerSettings.Authentication.Schemes = AuthenticationSchemes.None;
            //     options.ListenerSettings.Authentication.AllowAnonymous = true;
            // });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole(LogLevel.Debug);

            var applicationLogger = loggerfactory.CreateLogger("Application");

            app.Run(async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes("Hello World: " + DateTime.Now);
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Goodbye", CancellationToken.None);
                    webSocket.Dispose();
                }
                else
                {
                    context.Response.OnStarting(OnStarting, applicationLogger);
                    context.Response.OnCompleted(OnComplete, applicationLogger);

                    if (context.Request.Path.Value.StartsWith("/digest"))
                    {
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(context.GetDigest());
                    }
                    else if (context.Request.Path == "/divid")
                    {
                        int dividend, divisor;

                        if (int.TryParse(context.Request.Query["dividend"], out dividend) &&
                            int.TryParse(context.Request.Query["divisor"], out divisor))
                        {
                            if (divisor == 0)
                            {
                                // divisor == 0 will cause exception thrown from application
                                throw new DivideByZeroException("divisor");
                            }

                            var result = dividend / divisor;

                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync($"{dividend} / {divisor} = {result}");
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync("Incorrect dividend or divisor in query string.");
                        }
                    }
                }
            });
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseWebListener(options =>
                {
                    options.ListenerSettings.Authentication.Schemes = AuthenticationSchemes.None;
                    options.ListenerSettings.Authentication.AllowAnonymous = true;
                })
                .Build();

            host.Run();
        }

        private static Task OnStarting(object logger)
        {
            (logger as ILogger)?.LogInformation("On respond starting");
            return Task.FromResult(0);
        }

        private static Task OnComplete(object logger)
        {
            (logger as ILogger)?.LogInformation("On respond complete");
            return Task.FromResult(0);
        }
    }
}
