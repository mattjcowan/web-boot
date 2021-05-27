using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot
{
    public abstract class ConfigureServicesBase : IConfigureServices, IHasPriority
    {
        public virtual int Priority => 0;

        public abstract void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IWebHostEnvironment env);
    }
}