using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog.Sinks.Logz.Io.AspNetCoreApi.Logging;

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
                .UseLogzIoSerilog()
                .UseStartup<Startup>();
    }
}
