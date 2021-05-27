using System;
using Microsoft.Extensions.Hosting;
using Web.Boot;

namespace TestWebApp
{
    public class Program
    {
        internal static DateTime StartTime { get; private set; }

        public static void Main(string[] args)
        {
            StartTime = DateTime.UtcNow;
            HostBuilderFactory.CreateHostBuilder(args).Build().Run();
        }
    }
}