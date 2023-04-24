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
        services.AddInfrastructureServices();

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Resolve a service and use it
        //var service1 = serviceProvider.GetService<IService1>();

        Console.WriteLine("Application started.");
        var service = serviceProvider.GetService<IF5Service>();

        DCTData extractedDCTs, originalDCTs;

        string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\micpic.jpg"; // replace with the path to your JPEG image file
        string filePathExtract = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\ljubavic.jpg"; // replace with the path to your JPEG image file
        //string filePathExtract = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\ljubavicMalic-OUTPUT-14042023052620.jpg"; // replace with the path to your JPEG image file
        //string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\ljubavicMalic.jpg"; // replace with the path to your JPEG image file
        Image image = Image.FromFile(filePath);

        //var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\micpic-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\ljubavicMalic-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        using (FileStream fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                originalDCTs = service.Embed(image, "test", "hidden message", binaryWriter);
            }
        }

        using (FileStream fileStream = new FileStream(filePathExtract, FileMode.OpenOrCreate, FileAccess.Read))
        {
            using (BinaryReader binaryReader = new BinaryReader(fileStream))
            {
                extractedDCTs = service.ExtractDCT("test", binaryReader);
            }
        }

        CheckIfEqual(originalDCTs, extractedDCTs);
    }

    private static void CheckIfEqual(DCTData originalDCTs, DCTData extractedDCTs)
    {
        var missmatchCount = 0;
        List<Tuple<float, float>> missmathces = new List<Tuple<float, float>>();
        var mcuCount = originalDCTs.YDCTData.Length;
        for(int i = 0; i < mcuCount; i++)
        {
            for(int j = 0; j < 64; j++)
            {
                if (originalDCTs.YDCTData[i][j] != extractedDCTs.YDCTData[i][j])
                {
                    missmatchCount++;
                    missmathces.Add(new Tuple<float, float>(originalDCTs.YDCTData[i][j], extractedDCTs.YDCTData[i][j]));
                }
                if (originalDCTs.CBDCTData[i][j] != extractedDCTs.CBDCTData[i][j])
                {
                    missmatchCount++;
                    missmathces.Add(new Tuple<float, float>(originalDCTs.CBDCTData[i][j], extractedDCTs.CBDCTData[i][j]));
                }
                if (originalDCTs.CRDCTData[i][j] != extractedDCTs.CRDCTData[i][j])
                {
                    missmatchCount++;
                    missmathces.Add(new Tuple<float, float>(originalDCTs.CRDCTData[i][j], extractedDCTs.CRDCTData[i][j]));
                }
            }
        }
    }
}