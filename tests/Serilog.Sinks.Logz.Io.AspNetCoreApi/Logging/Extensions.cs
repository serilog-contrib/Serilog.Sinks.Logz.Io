using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;

namespace Serilog.Sinks.Logz.Io.AspNetCoreApi.Logging
{
    public static class Extensions
    {
        public static IWebHostBuilder UseLogzIoSerilog(this IWebHostBuilder builder)
        {
            return builder
                .UseSerilog((context, configuration) =>
                {
                    Debugging.SelfLog.Enable(msg => Trace.WriteLine(msg));

                    configuration.ReadFrom.Configuration(context.Configuration);
                    configuration.Enrich.With(new ServiceEnricher(context.Configuration));

                    Log.Information("Application is starting.");
                }, true);
        }
    }
}