using System.Linq;
using System.Text;
using HappyCode.NetCoreBoilerplate.Api.BackgroundServices;
using HappyCode.NetCoreBoilerplate.Api.Infrastructure.Configurations;
using HappyCode.NetCoreBoilerplate.Api.Infrastructure.Filters;
using HappyCode.NetCoreBoilerplate.Api.Infrastructure.Middlewares;
using HappyCode.NetCoreBoilerplate.Api.Infrastructure.OpenApi;
using HappyCode.NetCoreBoilerplate.BooksModule;
using HappyCode.NetCoreBoilerplate.ExamsModule;
using HappyCode.NetCoreBoilerplate.Core;
using HappyCode.NetCoreBoilerplate.Core.Providers;
using HappyCode.NetCoreBoilerplate.Core.Registrations;
using HappyCode.NetCoreBoilerplate.Core.Services;
using HappyCode.NetCoreBoilerplate.Core.Settings;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Scalar.AspNetCore;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Repositories;
using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace HappyCode.NetCoreBoilerplate.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSerilog();
            services.AddSingleton<ExceptionMiddleware>();

            services
                .AddHttpContextAccessor()
                .AddRouting(options => options.LowercaseUrls = true);

            services.AddControllers(options =>
                {
                    options.Filters.Add<HttpGlobalExceptionFilter>();
                    options.Filters.Add<ApiKeyAuthorizationFilter>();
                })
                .AddApplicationPart(Assembly.GetExecutingAssembly())
                .AddApplicationPart(typeof(ExamsModuleConfiguration).Assembly)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            //there is a difference between AddDbContext() and AddDbContextPool(), more info https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-2.0#dbcontext-pooling and https://stackoverflow.com/questions/48443567/adddbcontext-or-adddbcontextpool
            services.AddDbContext<EmployeesContext>(options => options.UseSqlServer(_configuration.GetConnectionString("MsSqlDb")), contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);
            services.AddDbContextPool<CarsContext>(options => options.UseSqlServer(_configuration.GetConnectionString("MsSqlDb")), poolSize: 10);
            services.AddDbContext<LimitKursContext>(options => options.UseSqlServer(_configuration.GetConnectionString("MsSqlDb")), contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);

            services.Configure<ApiKeySettings>(_configuration.GetSection("ApiKey"));
            services.AddOpenApi(_configuration);

            services.Configure<PingWebsiteSettings>(_configuration.GetSection("PingWebsite"));
            services.AddHttpClient(nameof(PingWebsiteBackgroundService));
            services.AddHostedService<PingWebsiteBackgroundService>();
            services.AddSingleton(x => x.GetServices<IHostedService>().OfType<IPingService>().Single());

            services.AddCoreComponents();
            services.AddBooksModule(_configuration);
            services.AddExamsModule(_configuration);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "HappyCode.NetCoreBoilerplate API", Version = "v1" });
                
                // Include XML comments from API project
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                // Include XML comments from ExamsModule
                var examsXmlFile = $"{typeof(ExamsModuleConfiguration).Assembly.GetName().Name}.xml";
                var examsXmlPath = Path.Combine(AppContext.BaseDirectory, examsXmlFile);
                options.IncludeXmlComments(examsXmlPath);

                // Include XML comments from AnnouncementsModule
                var announcementsXmlFile = $"{typeof(HappyCode.NetCoreBoilerplate.AnnouncementsModule.AnnouncementsContext).Assembly.GetName().Name}.xml";
                var announcementsXmlPath = Path.Combine(AppContext.BaseDirectory, announcementsXmlFile);
                options.IncludeXmlComments(announcementsXmlPath);

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _configuration["Jwt:Issuer"],
                        ValidAudience = _configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]))
                    };
                });

            services.AddAuthorization();

            services.AddFeatureManagement();

            var healthChecksBuilder = services.AddHealthChecks()
                .AddBooksModule(_configuration);
            if (_configuration.GetValue<bool>($"FeatureManagement:{FeatureFlags.DockerCompose}"))
            {
                healthChecksBuilder
                    .AddSqlServer(_configuration.GetConnectionString("MsSqlDb"), tags: ["ready"]);
            }

            // Register Announcements module
            services.AddDbContext<AnnouncementsContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<INotificationService, FirebaseNotificationService>();
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddlewareForFeature<ConnectionInfoMiddleware>(FeatureFlags.ConnectionInfo.ToString());

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "HappyCode.NetCoreBoilerplate API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz/live", new HealthCheckOptions
                {
                    Predicate = _ => false,
                }).ShortCircuit();
                endpoints.MapHealthChecks("/healthz/ready", new HealthCheckOptions
                {
                    Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }).ShortCircuit();

                endpoints.MapGet("/version", (VersionProvider provider) => provider.VersionEntries)
                    .ExcludeFromDescription();

                endpoints.MapControllers();
                endpoints.MapBooksModule();
                endpoints.MapExamsModule();

                endpoints.MapOpenApi()
                    .CacheOutput();

                // Scalar dokümantasyonu için özel bir endpoint yok, OpenAPI endpointi yeterli.
            });

            app.InitBooksModule();
            app.InitExamsModule();
        }
    }
}
