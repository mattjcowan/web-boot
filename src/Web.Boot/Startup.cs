using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Web.Boot
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/{pathInfo}", async context =>
                {
                    var pathInfo = context.Request.RouteValues["pathInfo"];
                    await context.Response.WriteAsync($"Hello from boot! (at /{pathInfo})");
                });
            });
        }
    }
}