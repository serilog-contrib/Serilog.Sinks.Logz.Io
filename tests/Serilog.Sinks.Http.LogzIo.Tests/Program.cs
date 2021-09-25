using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog.Sinks.Logz.Io;

namespace Serilog.Sinks.Http.LogzIo.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                RunTests();
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Application failed to start");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSerilog((context, configuration) =>
                    {
                        configuration.ReadFrom.Configuration(context.Configuration);
                    }, true);

                    /*
                    webBuilder.UseSerilog((context, configuration) =>
                    {
                        configuration
                            .WriteTo.LogzIoDurableHttp(
                                "https://listener-eu.logz.io:8071/?type=dev&token=<token>",
                                logzioTextFormatterOptions: new LogzioTextFormatterOptions
                                {
                                    BoostProperties = true,
                                    IncludeMessageTemplate = true,
                                    LowercaseLevel = true
                                })
                            .MinimumLevel.Verbose();
                    }, true);
                    */

                    Debugging.SelfLog.Enable(Console.WriteLine);

                    webBuilder.UseStartup<Startup>();
                });

        public static void RunTests()
        {
            var sample = new Sample
            {
                Type = typeof(Sample)
            };

            var result = LogzIoSerializer.Instance.Serialize(sample);
            Console.WriteLine(result);
        }

        private class Sample
        {
            public Type Type { get; set; }
        }
    }
}
