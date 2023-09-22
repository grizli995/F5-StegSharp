using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StegSharp.Application.Common.Interfaces;
using StegSharp.Infrastructure;

namespace Benchmarks.Application.Services
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class F5ServiceBenchmarks
    {
        private IF5Service _f5Service;
        private Image _imageMicpic, _imageLjubavic, _imageLjubavicMalic;

        public F5ServiceBenchmarks()
        {
        }
        
        [Benchmark]
        public void EmbedBenchmark()
        { 
            var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\Benchmark\\BENCHMARK-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
            using (FileStream fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    _f5Service.Embed(_imageMicpic, "test", "hidden message", binaryWriter);
                    binaryWriter.Close();
                }
            }
        }

        [GlobalSetup]
        public void Setup()
        {

            // Create a new instance of the ServiceCollection class
            var services = new ServiceCollection();

            // Register the services provided by the class library projects
            services.AddF5Services();

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve a service and use it
            //var service1 = serviceProvider.GetService<IService1>();

            Console.WriteLine("Application started.");
            _f5Service = serviceProvider.GetService<IF5Service>();

            LoadTestImages();
        }

        private void LoadTestImages()
        {
            string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\micpic.jpg";
            _imageMicpic = Image.Load<Rgba32>(filePath);

            filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\ljubavic.jpg";
            _imageLjubavic = Image.Load<Rgba32>(filePath);

            filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\ljubavicMalic.jpg";
            _imageLjubavicMalic = Image.Load<Rgba32>(filePath);
        }
    }
}
