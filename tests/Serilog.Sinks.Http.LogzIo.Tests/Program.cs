using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog.Sinks.Logz.Io;

namespace Serilog.Sinks.Http.LogzIo.Tests;

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
                webBuilder.UseStartup<Startup>();
            })
            .UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);

                Debugging.SelfLog.Enable(Console.WriteLine);
            }, true);

    public static void RunTests()
    {
        var sample = new Sample
        {
            Type = typeof(Sample)
        };

        var result = LogzIoSerializer.Instance.Serialize(sample);
        Console.WriteLine(result);
    }

    private sealed class Sample
    {
        public Type Type { get; set; }
    }
}