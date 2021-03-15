using Microsoft.Extensions.Hosting;

namespace Web.Boot
{
    public static class HostBuilderFactory
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureWebHost();
                });
    }
}
