using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot.Actions
{
    public class UseAuthenticationAction : ConfigureAppBase, IConfigureServices
    {
        public override int Priority => (int) KnownMiddlewarePriorities.Authentication;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IWebHostEnvironment env)
        {
            if (!Startup.ContainsMatchingContributingTypes<IConfigureAuthenticationOptions>())
                return;

            services.AddAuthentication(options =>
            {
                Startup.ConfigureWithContributingTypesInternal<IConfigureAuthenticationOptions>(e =>
                {
                    e.Configure(options);
                });
            });
        }

        public override void Configure(IApplicationBuilder app)
        {
            if (!Startup.ContainsMatchingContributingTypes<IConfigureAuthenticationOptions>())
                return;

            app.UseAuthentication();
        }
    }
}