using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Serilog.Sinks.Logz.Io.AspNetCoreApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration), true)
                .UseStartup<Startup>();
    }
}
