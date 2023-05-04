using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
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
        services.AddF5Services();

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Resolve a service and use it
        //var service1 = serviceProvider.GetService<IService1>();

        Console.WriteLine("Application started.");
        var service = serviceProvider.GetService<IF5Service>();

        DCTData extractedDCTs, originalDCTs;

        //string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\micpic.jpg"; // replace with the path to your JPEG image file
        //string filePathExtract = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\ljubavic.jpg"; // replace with the path to your JPEG image file
        string filePathExtract = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\jo-Stego-OUTPUT-03052023063706.jpg"; // replace with the path to your JPEG image file
        //string filePathExtract = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\ljubavicMalic-OUTPUT-14042023052620.jpg"; // replace with the path to your JPEG image file
        string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\jo.jpg"; // replace with the path to your JPEG image file
        //string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\ljubavicMalic.jpg"; // replace with the path to your JPEG image file
        Image image = Image.FromFile(filePath);

        //var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\micpic-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        //var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\ljubavicMalic-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\jo-Stego-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        using (FileStream fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                var msg = "";
                //originalDCTs = service.Embed(image, "test", "Laza voli Jovanu!! NAJVISE NA CELOM SVETU BREE!", binaryWriter);
                try
                {
                    service.Embed(image, "testovic", msg, binaryWriter);
                }
                catch (CapacityException ce)
                {
                    Console.WriteLine("uga buga");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Generalni uga buga");
                }
            }
        }

        using (FileStream fileStream = new FileStream(filePathExtract, FileMode.OpenOrCreate, FileAccess.Read))
        {
            using (BinaryReader binaryReader = new BinaryReader(fileStream))
            {
                var msg = service.Extract("testovic", binaryReader);
            }
        }
    }
}