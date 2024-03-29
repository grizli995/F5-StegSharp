﻿using StegSharp.Application.Common.Interfaces;
using StegSharp.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace StegSharp.Infrastructure
{
    public static class F5StegServiceCollectionExtensions
    {
        public static IServiceCollection AddF5Services(this IServiceCollection services) 
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
            services.AddScoped<IMCUConverterService, MCUConverterService>();
            services.AddScoped<IF5EmbeddingService, F5EmbeddingService>();
            services.AddScoped<IPermutationService, PermutationService>();
            services.AddScoped<IF5ParameterCalculatorService, F5ParameterCalculatorService>();
            services.AddScoped<IF5ExtractingService, F5ExtractingService>();

            return services;
        }
    }
}