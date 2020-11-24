using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using DeepLinking.Abstraction;
using DeepLinking.Helper;
using DeepLinking.Helper.CronJobServices;
using DeepLinking.Helper.CronJobServices.CronJobExtensionMethods;
using DeepLinking.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeepLinking
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCronJob<AnalyticSynced>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                //c.CronExpression = @"*/1 * * * * *";
                c.CronExpression = @"0 3 1 */2 *"; //  Run every 60 days at 3 AM
            });

            services.AddControllersWithViews();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<Dependencies>(Configuration.GetSection("Dependencies"));
            services.AddRazorPages();

            // services.AddHttpsRedirection(options =>
            // {
            //     options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            //     options.HttpsPort = 443;
            // });

            // services.AddHsts(options =>
            // {
            //     options.Preload = false;
            //     options.IncludeSubDomains = false;
            //     options.MaxAge = TimeSpan.FromDays(30);
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                     name: "default",
                     pattern: "{id}",
                     defaults: new { controller = "Link", action = "Index" });
            });
        }
    }
}
