using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Serilog.Sinks.Logz.Io.AspNetCoreApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            logger.LogDebug("DEBUG ASP.NET Core message");
            logger.LogInformation("INFORMATION ASP.NET Core message {CurrentTime}", DateTime.UtcNow);
            logger.LogInformation("Current person {@Person}", new Person());
            logger.LogWarning("WARNING ASP.NET Core message");
            logger.LogError("ERROR ASP.NET Core message");
            logger.LogCritical("FATAL ASP.NET Core message");
        }
    }

    public class Person
    {
        public string FirstName { get; set; } = "Mantas";
        public int Age { get; set; } = 21;
    }
}
