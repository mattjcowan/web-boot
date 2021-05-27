using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Web.Boot.Actions
{
    public class UseHttpsRedirectionAction : ConfigureAppBase
    {
        public override int Priority => (int) KnownMiddlewarePriorities.HttpsRedirection;

        public override void Configure(IApplicationBuilder app)
        {
            var bootConfig = app.ApplicationServices.GetRequiredService<BootConfig>();
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            if (!env.IsDevelopment() && bootConfig.EnableHttpsRedirection) app.UseHttpsRedirection();

            if (bootConfig.RewriteSchemeToHttps)
                app.Use(async (ctx, next) =>
                {
                    ctx.Request.Scheme = "https";
                    await next();
                });
        }
    }
}