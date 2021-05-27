using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot.Actions
{
    public class UseAuthorizationAction : ConfigureAppBase, IConfigureServices
    {
        public override int Priority => (int) KnownMiddlewarePriorities.Authorization;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IWebHostEnvironment env)
        {
            if (!Startup.ContainsMatchingContributingTypes<IConfigureAuthorizationOptions>())
                return;

            services.AddAuthorization(options =>
            {
                Startup.ConfigureWithContributingTypesInternal<IConfigureAuthorizationOptions>(e =>
                {
                    e.Configure(options);
                });
            });
        }

        public override void Configure(IApplicationBuilder app)
        {
            if (!Startup.ContainsMatchingContributingTypes<IConfigureAuthorizationOptions>())
                return;

            app.UseAuthorization();
        }
    }
}