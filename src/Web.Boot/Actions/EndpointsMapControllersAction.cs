using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot.Actions
{
    public class EndpointsMapControllersAction : ConfigureEndpointsBase
    {
        public override int Priority => -1;
        public override void Configure(IEndpointRouteBuilder endpoints)
        {
            var bootConfig = endpoints.ServiceProvider.GetRequiredService<BootConfig>();
            if (!bootConfig.EnableControllers) return;

            endpoints.MapControllers();
        }
    }
}