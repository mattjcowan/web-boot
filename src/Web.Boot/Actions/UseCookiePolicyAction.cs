using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot.Actions
{
    public class UseCookiePolicyAction : ConfigureAppBase
    {
        public override int Priority => (int) KnownMiddlewarePriorities.CookiePolicy;

        public override void Configure(IApplicationBuilder app)
        {
            var bootConfig = app.ApplicationServices.GetRequiredService<BootConfig>();
            if (!bootConfig.EnableCookiePolicy) return;

            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = configuration.AsEnum("boot:cookies:HttpOnly", HttpOnlyPolicy.Always),
                MinimumSameSitePolicy = configuration.AsEnum("boot:cookies:MinimumSameSitePolicy", SameSiteMode.Lax),
                Secure = configuration.AsEnum("boot:cookies:Secure", CookieSecurePolicy.Always)
            });
        }
    }
}