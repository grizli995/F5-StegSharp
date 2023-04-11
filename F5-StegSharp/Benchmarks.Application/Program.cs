using Application.Common.Interfaces;
using BenchmarkDotNet.Running;
using Benchmarks.Application.Services;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static void Main(string[] args)
    {

        BenchmarkRunner.Run<F5ServiceBenchmarks>();
    }
}