using System.Net.WebSockets;
using Web.Boot;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Extension
{
    public class ConfigureServicesAction: ConfigureServicesBase
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IWebHostEnvironment env)
        {

        }

    }
}