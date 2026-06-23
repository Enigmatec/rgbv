using BackgroundServices.Jobs;
using Core.Data;
using Core.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Service;
using Service.Helpers;
using Service.Implementation;
using Service.Implementations;
using Service.Interfaces;
using Service.StringKeys;
using System;
using System.Configuration;
using System.Text;
using System.Text.Json.Serialization;
using Coravel;
using Service.AppServices;
using Service.Invocables;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using Hangfire;
using Hangfire.SqlServer;

namespace Api.Helpers
{
    /// <summary>
    /// Abstracts all dependencies and configuration for the start up class
    /// </summary>
    public static class Dependency
    {
        /// <summary>
        /// configures the API behavour for requests and responses
        /// </summary>
        /// <param name="services"></param>
        public static void AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            //adds a suppression to the default model state invalid filter.
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            //adds a custom controller model state validation filter
            services.AddControllers(options => options.Filters.Add(typeof(ModelValidationFilter)))

            //adds the new microsoft json options for C# to JSON converters
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            //adds the oold Newtonsoft json options for C# to JSON converters
             .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            //set the Imemory cache
            services.AddMemoryCache();

            //add cross-orgin resource sharing
            services.AddCors(options =>
            {
                options.AddPolicy(StartupKeys.ReportGBV, builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition")
                    .Build();
                });
            });

            //add swagger api documentation
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = StartupKeys.ReportGBV, Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert token into the field eg 'Bearer {token}' ... ",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                });

                //var filepath = Path.Combine(AppContext.BaseDirectory, "Api.xml");
                //c.IncludeXmlComments(filepath);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id= "Bearer"
                            }
                        } , Array.Empty<string>()
                    }
                });
            });

            var redisConfig = configuration.GetSection("Redis").Get<RedisConfiguration>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = new ConfigurationOptions
                {
                    Password = redisConfig.Password,
                    EndPoints = { $"{redisConfig.Hosts[0].Host}:{redisConfig.Hosts[0].Port}" },
                };
                options.InstanceName = configuration.GetValue<bool>(StartupKeys.IsLive) ? "live_" : "test_";
                // options.Configuration = "redis-17867.c282.east-us-mz.azure.cloud.redislabs.com:17867";
            });

            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>((_) => configuration.GetSection("Redis").Get<RedisConfiguration>());

            services.AddQueue();
        }

        /// <summary>
        /// Setup the databases settings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="Configuration"></param>
        public static void AddDatabaseConfiguration(this IServiceCollection services, IConfiguration Configuration)
        {
            //set the database to use SQL Server.

            string connection = !Configuration.GetValue<bool>(StartupKeys.IsLive) ? StartupKeys.DefaultConnection : StartupKeys.LiveConnection;

            Console.WriteLine(connection);

            services.AddDbContext<ApplicationDbContext>(
              options => options.UseSqlServer(Configuration.GetConnectionString(connection),
              p =>
              {
                  p.EnableRetryOnFailure();
                  p.MaxBatchSize(150);
                  p.CommandTimeout(900000);
              }));

            services.AddIdentity<ApplicationUser, ApplicationRole>(
               options =>
               {
                   // Password settings.
                   options.Password.RequireDigit = false;
                   options.Password.RequireLowercase = false;
                   options.Password.RequireNonAlphanumeric = false;
                   options.Password.RequireUppercase = false;
                   options.Password.RequiredLength = 6;
                   options.Password.RequiredUniqueChars = 1;
                   options.SignIn.RequireConfirmedEmail = true;
                   options.User.RequireUniqueEmail = true;
                   options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
               }
              ).AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();


            services.AddHangfire(configuration =>
            {
                var sqlOptions = new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                };

                configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                Configuration.GetConnectionString(connection),
                sqlOptions);
            });

            // Add the processing server as IHostedService
            services.AddHangfireServer();
        }

        /// <summary>
        /// Sets up the Configuration Setting and Authentication
        /// </summary>
        /// <param name="services"></param>
        /// <param name="Configuration"></param>
        public static void AddSettingsAndAuthentication(this IServiceCollection services, IConfiguration Configuration)
        {
            //configures SendGrid email service
            var sendGrid = Configuration.GetSection(nameof(SendGridSettings));
            services.Configure<SendGridSettings>(sendGrid);

            //set JWT
            var JwtSettingsSection = Configuration.GetSection(nameof(JwtSettings));
            services.Configure<JwtSettings>(JwtSettingsSection);

            var jwtSettings = JwtSettingsSection.Get<JwtSettings>();

            //Encode Secret Key
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

            //Add jwt authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
             {
                 options.SaveToken = true;
                 options.RequireHttpsMetadata = true;
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateAudience = false,
                     ValidateIssuer = true,
                     ValidateIssuerSigningKey = false,
                     ValidateLifetime = true,
                     ValidIssuer = jwtSettings.Site,
                     //ValidAudience = jwtSettings.Audience,
                     IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                     RequireExpirationTime = true,
                     ClockSkew = TimeSpan.Zero
                 };
             });

            //Set Authorization policies for the difference roles
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyKeys.CSOandSP, policy => policy.RequireRole(RoleKeys.CSO, RoleKeys.ServiceProvider, RoleKeys.CSOSupervior, RoleKeys.ServiceProviderSupervior, RoleKeys.StateAdministrator, RoleKeys.ImplementingPartner));
                options.AddPolicy(PolicyKeys.OrganisationSupervisor, policy => policy.RequireRole(RoleKeys.CSOSupervior, RoleKeys.ServiceProviderSupervior, RoleKeys.StateAdministrator, RoleKeys.LocalGovernment, RoleKeys.Administrator));
                options.AddPolicy(PolicyKeys.AllSuperVisors, policy => policy.RequireRole(RoleKeys.FederalSupervisor, RoleKeys.StateSupervisor, RoleKeys.StateAdministrator, RoleKeys.LocalGovernment));
                options.AddPolicy(PolicyKeys.CSOandAdmin,
                    policy => policy.RequireRole(
                        RoleKeys.Administrator,
                        RoleKeys.CSO,
                        RoleKeys.ServiceProvider,
                        RoleKeys.CSOSupervior,
                        RoleKeys.ServiceProviderSupervior,
                        RoleKeys.StateAdministrator));
                options.AddPolicy(PolicyKeys.Donor, policy => policy.RequireRole(RoleKeys.Donor));
                //RoleKeys.FederalSupervisor,//*
                //RoleKeys.StateSupervisor, //*
                //RoleKeys.LocalGovernment));//*
            }
             );
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddTransient<IRazorViewToStringRenderer, RazorViewRendererService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<INotification, NotificationService>();

            services.AddScoped<IAuthentication, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOrganisationService, OrganisationService>();
            services.AddScoped<ICaseService, CaseService>();
            services.AddScoped<IMetrics, MetricsService>();
            services.AddScoped<ISetting, SettingsService>();
            services.AddScoped<IDashboardService, DashBoardService>();

            services.AddScoped<IServicesProvidedService, ServicesProvidedService>();
            services.AddScoped<IDonorService, DonorServices>();
            //Adds the Background service jobs
            services.AddHostedService<DailyReportSummaryJob>();
            services.AddHostedService<DailyStateReportSummaryJob>();

            services.AddScoped<IAppCachingService, AppCachingService>();

            services.AddTransient<ClearDashboardCacheInvocable>();

            services.Scan(scan =>
                   scan.FromAssemblyOf<IBusinessService>()
                   .AddClasses(c => c.AssignableTo<IBusinessService>())
                   .AsSelf()
                   .WithScopedLifetime());
        }
    }
}