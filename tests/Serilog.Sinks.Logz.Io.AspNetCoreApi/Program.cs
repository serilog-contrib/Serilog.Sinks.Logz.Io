using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Logz.Io.AspNetCoreApi;
using Serilog.Sinks.Logz.Io.AspNetCoreApi.Logging;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseLogzIoSerilog();
    var startup = new Startup(builder.Configuration);
    startup.ConfigureServices(builder.Services);
    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Startup>>();
    var env = app.Services.GetRequiredService<IWebHostEnvironment>();
    startup.Configure(app, env, logger);
    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception e)
{
    Environment.ExitCode = 1;
    Log.Fatal(e, "Application Execution Error");
    throw;
}
