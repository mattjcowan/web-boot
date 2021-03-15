using Microsoft.Extensions.Hosting;
using Web.Boot;

namespace web_boot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostBuilderFactory.CreateHostBuilder(args).Build().Run();
        }
    }
}
