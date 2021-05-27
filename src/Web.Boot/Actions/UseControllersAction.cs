using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot.Actions
{
    public class UseControllersAction : ConfigureServicesBase
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            var bootConfig = configuration.GetBootConfig();
            if (!bootConfig.EnableControllers) return;

            var c = services.AddControllers();

            foreach (var a in Startup.ContributingAssemblies)
                c.AddApplicationPart(a);
        }
    }
}