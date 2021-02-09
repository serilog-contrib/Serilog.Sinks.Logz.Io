using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Sinks.Logz.Io.AspNetCoreApi.Middleware;

namespace Serilog.Sinks.Logz.Io.AspNetCoreApi
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<LogScopeMiddleware>();
            app.UseMvc();

            _logger.LogDebug("DEBUG ASP.NET Core message");
            _logger.LogInformation("INFORMATION ASP.NET Core message {CurrentTime}", DateTime.UtcNow);
            _logger.LogInformation("Current person {@Person}", new Person());
            _logger.LogWarning("WARNING ASP.NET Core message");
            _logger.LogError("ERROR ASP.NET Core message");
            _logger.LogCritical("FATAL ASP.NET Core message");
        }
    }

    public class Person
    {
        public string FirstName { get; set; } = "Mantas";
        public int Age { get; set; } = 21;
    }
}
