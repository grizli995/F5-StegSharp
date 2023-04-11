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
        services.AddInfrastructureServices();

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Resolve a service and use it
        //var service1 = serviceProvider.GetService<IService1>();

        Console.WriteLine("Application started.");
        var service = serviceProvider.GetService<IF5Service>();



        //string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\micpic.jpg"; // replace with the path to your JPEG image file
        string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\ljubavicMalic.jpg"; // replace with the path to your JPEG image file
        Image image = Image.FromFile(filePath);

        //var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\micpic-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\ljubavicMalic-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        using (FileStream fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                service.Embed(image, "test", "hidden message", binaryWriter);
            }
        }

    }
}