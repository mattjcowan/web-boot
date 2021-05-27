using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot.Actions
{
    public class UseForwardedHeadersAction : ConfigureAppBase, IConfigureServices
    {
        public override int Priority => int.MinValue;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IWebHostEnvironment env)
        {
            var bootConfig = configuration.GetBootConfig();
            if (!bootConfig.EnableForwardedHeaders) return;

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        public override void Configure(IApplicationBuilder app)
        {
            var bootConfig = app.ApplicationServices.GetRequiredService<BootConfig>();
            if (!bootConfig.EnableForwardedHeaders) return;

            app.UseForwardedHeaders();
        }
    }
}