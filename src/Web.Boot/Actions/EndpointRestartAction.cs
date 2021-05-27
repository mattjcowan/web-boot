using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Web.Boot.Actions
{
    public class EndpointRestartAction : ConfigureEndpointsBase
    {
        public override int Priority => int.MaxValue;

        public override void Configure(IEndpointRouteBuilder endpoints)
        {
            var env = endpoints.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var configuration = endpoints.ServiceProvider.GetRequiredService<IConfiguration>();
            var bootConfig = endpoints.ServiceProvider.GetRequiredService<BootConfig>();
            if (!bootConfig.EnableRestart) return;

            var restartPath = configuration.AsString("boot:restart:path", "/restart");
            var restartMethod = configuration.AsString("boot:restart:method", "GET");
            var restartKey = configuration.AsString("boot:restart:key", "boot");

            if (restartMethod.Equals("post", StringComparison.InvariantCultureIgnoreCase))
            {
                endpoints.MapPost(restartPath, async context =>
                {
                    var requestKey = await ExtractRequestKeyAsync(context, true);
                    await ExecuteRestart(requestKey, restartKey, context, env.IsDevelopment());
                });
            }
            else
            {
                endpoints.MapGet(restartPath, async context =>
                {
                    var requestKey = await ExtractRequestKeyAsync(context, false);
                    await ExecuteRestart(requestKey, restartKey, context, env.IsDevelopment());
                });
            }
        }

        private static async Task ExecuteRestart(string requestKey, string restartKey, HttpContext context,
            bool isDevelopment)
        {
            if (string.IsNullOrWhiteSpace(restartKey) || requestKey == restartKey)
            {
                // assume in development, the developer is using dotnet watch (just modify the timestamp of a file)
                if (isDevelopment && File.Exists("Program.cs"))
                {
                    File.SetLastWriteTime("Program.cs", DateTime.Now);
                } 
                else
                {
                    context.RequestServices.GetRequiredService<IHostApplicationLifetime>().StopApplication();
                }

                await context.Response.WriteAsync($"Restarting ...");
            }
            else
            {
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                await context.Response.WriteAsync($"Bad request ...");
            }
        }


        private static async Task<string> ExtractRequestKeyAsync(HttpContext context, bool deserializeBody = false)
        {
            // try to get the key from the header
            string requestKey = null;
            if(context.Request.Headers.TryGetValue("x-key", out var value))
                requestKey = value.FirstOrDefault();

            if(string.IsNullOrWhiteSpace(requestKey) && deserializeBody)
            {
                string bodyJson;
                using (var reader
                    = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyJson = await reader.ReadToEndAsync();
                }
                var restartRequest = JsonSerializer.Deserialize<RestartRequest>(bodyJson);
                requestKey = restartRequest?.Key ?? "";
            }

            if(string.IsNullOrWhiteSpace(requestKey))
                requestKey = context.Request.Query["key"].FirstOrDefault();

            return requestKey;
        }

        class RestartRequest
        {
            public string Key { get; set; }
        }
    }
}