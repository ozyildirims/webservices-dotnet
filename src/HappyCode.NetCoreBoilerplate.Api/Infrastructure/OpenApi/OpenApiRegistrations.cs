using System.Diagnostics.CodeAnalysis;
using HappyCode.NetCoreBoilerplate.Api.Infrastructure.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using HappyCode.NetCoreBoilerplate.ExamsModule;

namespace HappyCode.NetCoreBoilerplate.Api.Infrastructure.OpenApi
{
    [ExcludeFromCodeCoverage]
    public static class OpenApiRegistrations
    {
        public static void AddOpenApi(this IServiceCollection services, IConfiguration configuration)
        {
            string secretKey = configuration.GetValue<string>("ApiKey:SecretKey");

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Simple Api",
                    Description = $"Authorization: ApiKey {secretKey}",
                    Contact = new OpenApiContact
                    {
                        Name = "Łukasz Kurzyniec",
                        Url = new Uri("https://kurzyniec.pl/"),
                    }
                });

                // Include XML comments from API project
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Include XML comments from ExamsModule
                var examsXmlFile = "HappyCode.NetCoreBoilerplate.ExamsModule.xml";
                var examsXmlPath = Path.Combine(AppContext.BaseDirectory, examsXmlFile);
                if (File.Exists(examsXmlPath))
                {
                    options.IncludeXmlComments(examsXmlPath);
                }

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

                options.OperationFilter<FeatureFlagOperationTransformer>();
                options.OperationFilter<SecurityRequirementOperationTransformer>();
                options.DocumentFilter<RemoveDeprecatedDocumentTransformer>();
                options.DocumentFilter<SecurityDefinitionDocumentTransformer>();
            });
        }
    }
}
