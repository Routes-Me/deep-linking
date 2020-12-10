using DeepLinking.Abstraction;
using DeepLinking.Helper;
using DeepLinking.Models;
using DeepLinking.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Threading.Channels;

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

            services.AddControllersWithViews();
            services.AddControllers();
            services.AddMvc().AddNewtonsoftJson();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<Channel<LinkLogs>>(Channel.CreateUnbounded<LinkLogs>(new UnboundedChannelOptions() { SingleReader = true }));
            services.AddSingleton<ChannelReader<LinkLogs>>(svc => svc.GetRequiredService<Channel<LinkLogs>>().Reader);
            services.AddSingleton<ChannelWriter<LinkLogs>>(svc => svc.GetRequiredService<Channel<LinkLogs>>().Writer);

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<Dependencies>(Configuration.GetSection("Dependencies"));
            services.AddRazorPages();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
           
            app.UseHttpsRedirection();
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
