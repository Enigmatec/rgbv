using Api.Configurations;
using Api.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Helpers;
using Service.StringKeys;
using System;
using System.Reflection;
using Coravel;
using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.Logging;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Hangfire.SqlServer;
using Serilog;

namespace Api
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
            services.AddApiConfiguration(Configuration);

            services.AddDatabaseConfiguration(Configuration);

            services.AddSettingsAndAuthentication(Configuration);

            services.AddServices();

            services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

            services.AddRazorPages();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline..
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(StartupKeys.ReportGBV);

            app.UseAuthentication();
            app.UseAuthorization();
            Job(app);
            var envString = !Configuration.GetValue<bool>(StartupKeys.IsLive) ? "Test" : "Prod";
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{StartupKeys.ReportGBV} V1 "
                                                              + TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")) + " " + $"Env: {envString}");
            });

            app.ApplicationServices.ConfigureQueue()
                .LogQueuedTaskProgress(app.ApplicationServices.GetService<ILogger<IQueue>>())
             .OnError(e => throw e);
            app.UseSerilogRequestLogging();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard();
            });
        }

        public void Job(IApplicationBuilder app)
        {
            try
            {
                app.UseHangfireDashboard(Configuration.GetSection("HangfireSettings:Route").Value, new DashboardOptions
                {
                    Authorization = new[]
                    {
                        new HangfireCustomBasicAuthenticationFilter
                        {
                             User = Configuration.GetSection("HangfireSettings:Credentials:User").Value,
                             Pass = Configuration.GetSection("HangfireSettings:Credentials:Password").Value
                        }
                    },
                    AppPath = Configuration.GetSection("HangfireSettings:Dashboard:AppPath").Value,
                    DashboardTitle = Configuration.GetSection("HangfireSettings:Dashboard:DashboardTitle").Value


                    //return app.UseHangfireDashboard();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}