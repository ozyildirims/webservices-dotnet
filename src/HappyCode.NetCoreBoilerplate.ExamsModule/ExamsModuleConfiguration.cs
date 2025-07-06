using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using HappyCode.NetCoreBoilerplate.ExamsModule.Services;
using HappyCode.NetCoreBoilerplate.ExamsModule.Repositories;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace HappyCode.NetCoreBoilerplate.ExamsModule;

[ExcludeFromCodeCoverage]
public static class ExamsModuleConfiguration
{
    public static IServiceCollection AddExamsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ExamsContext>(options => 
            options.UseSqlServer(configuration.GetConnectionString("MsSqlDb")));

        services.AddScoped<IExamDefinitionRepository, ExamDefinitionRepository>();
        services.AddScoped<IExamResultRepository, ExamResultRepository>();
        services.AddScoped<IExamDefinitionService, ExamDefinitionService>();
        services.AddScoped<IExamResultService, ExamResultService>();

        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    public static IEndpointRouteBuilder MapExamsModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        return endpoints;
    }

    public static async Task InitExamsModule(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ExamsContext>();
        await context.Database.MigrateAsync();
    }
} 