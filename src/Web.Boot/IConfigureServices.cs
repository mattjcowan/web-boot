using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot
{
    public interface IConfigureServices
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env);
    }


}