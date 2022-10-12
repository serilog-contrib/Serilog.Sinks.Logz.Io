using System;
using Microsoft.Extensions.Hosting;

namespace Serilog.Sinks.Logz.Io.AspNetCoreApi.Logging;

public static class Extensions
{
    public static IHostBuilder UseLogzIoSerilog(this IHostBuilder builder)
    {
        return builder
            .UseSerilog((context, configuration) =>
            {
                Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

                configuration.ReadFrom.Configuration(context.Configuration);
                configuration.Enrich.With(new ServiceEnricher(context.Configuration));

                Log.Information("Application is starting.");
            }, true);
    }
}