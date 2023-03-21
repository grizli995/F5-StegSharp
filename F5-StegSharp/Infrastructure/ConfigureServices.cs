using Application.Common.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services) 
        {
            services.AddScoped<IColorTransformationService, ColorTransformationService>();
            services.AddScoped<IF5Service, F5Service>();
            services.AddScoped<IDCTService, DCTService>();
            services.AddScoped<IPaddingService, PaddingService>();

            return services;
        }
    }
}