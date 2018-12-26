using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace Serilog.Sinks.Logz.Io.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // lets override to local logstash
            // LogzioSink.OverrideLogzIoUrl = "http://localhost:30000";

            var configuration = GetConfiguration(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .CreateLogger();

            Log.Logger.Debug("Application is starting..");
            
            Log.Logger.Debug("DEBUG message");
            Log.Logger.Information("INFORMATION message");
            Log.Logger.Warning("WARNING message");
            Log.Logger.Error("ERROR message");
            Log.Logger.Fatal("FATAL message");

            Log.Logger.Information("Press ENTER to exit");

            System.Console.ReadLine();
        }

        private static IConfigurationRoot GetConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddCommandLine(args ?? new string[0])
                .Build();
        }

    }
}
