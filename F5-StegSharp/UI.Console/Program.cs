using StegSharp.Application.Common.Exceptions;
using StegSharp.Application.Common.Interfaces;
using StegSharp.Application.Models;
using StegSharp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Collections.Generic;

public class Program
{
    private static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddF5Services();
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<IF5Service>();
        var histogramService = serviceProvider.GetService<IHistogramService>();
        Console.WriteLine("Application started.");

        string response;

        Console.WriteLine("Embed (e) or extract (ex) ?");
        response = Console.ReadLine();
        while (true)
        {
            if (response == "e")
            {
                Console.WriteLine("Please provide path to the image you want to hide the message in.");
                var imageFilePath = Console.ReadLine();
                Image image = Image.FromFile(imageFilePath);

                Console.WriteLine("Please provide a password that will be used for embeding the hidden data.");
                var password = Console.ReadLine();

                Console.WriteLine("Please provide the message that you want to hide in the image.");
                var message = Console.ReadLine();

                var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\h-Stego-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";

                using (FileStream fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                    {
                        try
                        {
                            service.Embed(image, password, message, binaryWriter);
                            Console.WriteLine("Message successfully embedded.");
                        }
                        catch (CapacityException ce)
                        {
                            Console.WriteLine(ce.Message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
            else if (response == "ex")
            {
                Console.WriteLine("Please provide path to the image you want to read the hidden message from.");
                var filePathExtract = Console.ReadLine();

                Console.WriteLine("Please provide a password that will be used for extracting the hidden data.");
                var password = Console.ReadLine();

                string msg = string.Empty;
                try
                {
                    using (FileStream fileStream = new FileStream(filePathExtract, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        using (BinaryReader binaryReader = new BinaryReader(fileStream))
                        {
                            msg = service.Extract(password, binaryReader);
                        }
                    }

                    Console.WriteLine(msg);
                }
                catch (MatrixEncodingException me)
                {
                    Console.WriteLine(me.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (response == "hist")
            {
                var imagePath = Console.ReadLine();

                var histValues = histogramService.GetHistogramFromImagePath(imagePath);
                string csvFilePath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\histogram-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.csv";
                using (var writer = new StreamWriter(csvFilePath))
                {
                    // Write header
                    writer.WriteLine("Key,Value,Color");
                    int i = 0;
                    string[] HexColorCodes = new string[]
                    {
                        "#FF0000",
                        "#FFA500",
                        "#FFFF00",
                        "#00FF00",
                        "#00FFFF",
                        "#0000FF",
                        "#8A2BE2",
                        "#FF00FF",
                        "#800080",
                        "#FFFFFF"
                    };
                    var negativeHisValues = histValues.Where(item => item.Key < 0).OrderBy(item => Math.Abs(item.Key)).ToDictionary(k => k.Key, v => v.Value);
                    var positiveHisValues = histValues.Where(item => item.Key >= 0).OrderBy(item => Math.Abs(item.Key)).ToDictionary(k => k.Key, v => v.Value);

                    // Write dictionary values
                    foreach (var pair in positiveHisValues)
                    {
                        if(pair.Key == 0)
                        {
                            writer.WriteLine($"{pair.Key},{pair.Value},#000000");
                        }
                        else
                        {
                            var colorIndex = i  % HexColorCodes.Count();
                            writer.WriteLine($"{pair.Key},{pair.Value},{HexColorCodes[colorIndex]}");
                        }
                        i++;
                    }
                    i = 1;
                    // Write dictionary values
                    foreach (var pair in negativeHisValues)
                    {
                        if (pair.Key == 0)
                        {
                            writer.WriteLine($"{pair.Key},{pair.Value},#000000");
                        }
                        else
                        {
                            var colorIndex = i %  HexColorCodes.Count();
                            writer.WriteLine($"{pair.Key},{pair.Value},{HexColorCodes[colorIndex]}");
                        }
                        i++;
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid option picked.");
            }
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Embed (e) or extract (ex) ?");
            response = Console.ReadLine();
        }
    }
}