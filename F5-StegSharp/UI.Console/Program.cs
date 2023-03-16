using Application.Common.Interfaces;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;

public class Program
{
    private static void Main(string[] args)
    {
        // Create a new instance of the ServiceCollection class
        var services = new ServiceCollection();

        // Register the services provided by the class library projects
        //services.AddSingleton<IService1, Service1>();
        //services.AddSingleton<IService2, Service2>();
        //services.AddSingleton<IService3, Service3>();

        // Build the service provider
        services.AddInfrastructureServices();
        var serviceProvider = services.BuildServiceProvider();

        // Resolve a service and use it
        //var service1 = serviceProvider.GetService<IService1>();
        //service1.DoSomething();

        Console.WriteLine("Application started.");
        var service = serviceProvider.GetService<IColorTransformationService>();

    }
}