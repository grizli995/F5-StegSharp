using StegSharp.Application.Common.Exceptions;
using StegSharp.Application.Common.Interfaces;
using StegSharp.Application.Models;
using StegSharp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;

public class Program
{
    private static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddF5Services();
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<IF5Service>();

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
            }
            else if (response == "ex")
            {
                Console.WriteLine("Please provide path to the image you want to read the hidden message from.");
                var filePathExtract = Console.ReadLine();

                Console.WriteLine("Please provide a password that will be used for extracting the hidden data.");
                var password = Console.ReadLine();

                string msg = string.Empty;
                using (FileStream fileStream = new FileStream(filePathExtract, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        msg = service.Extract(password, binaryReader);
                    }
                }

                Console.WriteLine(msg);
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

        ////string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\micpic.jpg"; // replace with the path to your JPEG image file
        ////string filePathExtract = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\ljubavic.jpg"; // replace with the path to your JPEG image file
        //string filePathExtract = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\h_test_whatsapp_slika.jpg"; // replace with the path to your JPEG image file
        ////string filePathExtract = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\ljubavicMalic-OUTPUT-14042023052620.jpg"; // replace with the path to your JPEG image file
        //string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\h.jpg"; // replace with the path to your JPEG image file
        ////string filePath = "C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\ljubavicMalic.jpg"; // replace with the path to your JPEG image file
        //Image image = Image.FromFile(filePath);

        ////var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\micpic-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        ////var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\ljubavicMalic-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        //var outPath = $"C:\\Files\\Faks\\Faks\\Diplomski rad\\Implementacija\\F5-StegSharp\\F5-StegSharp\\Output\\h-Stego-OUTPUT-{DateTime.UtcNow.ToString("ddMMyyyyhhmmss")}.jpg";
        //using (FileStream fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
        //{
        //    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
        //    {
        //        var msg = "Look again at that dot. That's here. That's home. That's us. On it everyone you love, everyone you know, everyone you ever heard of, every human being who ever was, lived out their lives. The aggregate of our joy and suffering, thousands of confident religions, ideologies, and economic doctrines, every hunter and forager, every hero and coward, every creator and destroyer of civilization, every king and peasant, every young couple in love, every mother and father, hopeful child, inventor and explorer, every teacher of morals, every corrupt politician, every \"superstar,\" every \"supreme leader,\" every saint and sinner in the history of our species lived there--on a mote of dust suspended in a sunbeam.";
        //        //originalDCTs = service.Embed(image, "test", "Laza voli Jovanu!! NAJVISE NA CELOM SVETU BREE!", binaryWriter);
        //        try
        //        {
        //            service.Embed(image, "test", msg, binaryWriter);
        //        }
        //        catch (CapacityException ce)
        //        {
        //            Console.WriteLine("uga buga");
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine("Generalni uga buga");
        //        }
        //    }
        //}

        ////using (FileStream fileStream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.Read))
        //using (FileStream fileStream = new FileStream(filePathExtract, FileMode.OpenOrCreate, FileAccess.Read))
        //{
        //    using (BinaryReader binaryReader = new BinaryReader(fileStream))
        //    {
        //        var msg = service.Extract("test", binaryReader);
        //    }
        //}
    }
}