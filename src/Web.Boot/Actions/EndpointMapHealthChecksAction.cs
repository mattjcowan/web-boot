using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot.Actions
{
    public class EndpointMapHealthChecksAction : ConfigureEndpointsBase, IConfigureServices
    {
        public override int Priority => 0;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IWebHostEnvironment env)
        {
            services.AddHealthChecks();
        }

        public override void Configure(IEndpointRouteBuilder endpoints)
        {
            // Configure the Health Check endpoint
            endpoints.MapHealthChecks("/healthz");
        }
    }
}