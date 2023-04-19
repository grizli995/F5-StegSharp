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
            services.AddScoped<IRunLengthEncodingService, RunLengthEncodingService>();
            services.AddScoped<IHuffmanEncodingService, HuffmanEncodingService>();
            services.AddScoped<IEncodingOrchestratorService, EncodingOrchestratorService>();
            services.AddScoped<IHeaderService, HeaderService>();
            services.AddScoped<IHuffmanDecodingService, HuffmanDecodingService>();
            services.AddScoped<IBitReaderService, BitReaderService>();

            return services;
        }
    }
}