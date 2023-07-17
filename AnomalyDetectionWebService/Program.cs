using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using AnomalyDetectionWebService.Controllers;
using System.Threading.Tasks;
using AnomalyDetectionWebService.Models;
using AnomalyDetectionWebService.Models.Types;
using AnomalyDetectionWebService.Models.Utils;

namespace AnomalyDetectionWebService
{
    // establish server to handle with requests for anomaly detection
    public class Program
    {
        public static readonly string NormalModelsDBFolder = "NormalModelsDB" + System.IO.Path.DirectorySeparatorChar;
        public static readonly string NormalModelsFileExtension = ".json";
        public static void Main(string[] args)
        {
            Console.WriteLine("Server is on.\n");

            // check the NormalModelsDBFolder exists
            if (!System.IO.Directory.Exists(NormalModelsDBFolder))
            {
                Console.WriteLine("Error: Unable to find folder "+ NormalModelsDBFolder);
                Console.WriteLine("       Which in use for database/IO for models storage");
                Console.WriteLine("\nPress Enter to abort.");
                Console.ReadLine();
                return;
            }

            // check this program can read\write to NormalModelsDBFolder
            try {
                string testFile = NormalModelsDBFolder + "testTestTest9999";
                System.IO.File.WriteAllText(testFile, "something");
                if (!System.IO.File.Exists(testFile)) throw new Exception();
                System.IO.File.Delete(testFile);
                if (System.IO.File.Exists(testFile)) throw new Exception();
            } catch 
            {
                Console.WriteLine("Error: Unable to write and delete within " + NormalModelsDBFolder);
                Console.WriteLine("       Which in use for database/IO for models storage");
                Console.WriteLine("\nPress Enter to abort.");
                Console.ReadLine();
                return;
            }

            // ask what to do with old normal models, if exist in the NormalModelsDBFolder
            if (System.IO.Directory.GetFiles(NormalModelsDBFolder, "*" + NormalModelsFileExtension).Length != 0)
            {
                // ask if old normal models should be load
                Console.WriteLine("Do you want to restore prev Normal Models?");
                Console.WriteLine($"y        yes");
                Console.WriteLine($"n        no, but they will stay in folder" + NormalModelsDBFolder);
                Console.WriteLine($"remove   no, and ALL (!) *{NormalModelsFileExtension} files in " + NormalModelsDBFolder + " will be removed!");
                Console.Write("Enter option [y/n/remove] : ");

                string input = Console.ReadLine().ToLower().Trim();
                while (input != "y" && input != "n" && input != "remove")
                {
                    Console.Write("Enter option [y/n/remove] : ");
                    input = Console.ReadLine().ToLower().Trim();
                }

                List<MODEL> initList = new List<MODEL>();

                // if we need to restore prev normal_model then load it from system-file
                // it's ok since it's only in the start of the server
                // and we take only the "info" field typeof MODEL which contains only 3 finite fields,
                // and therefore we won't need much memory for that.
                if (input == "y")
                {
                    try
                    {
                        foreach (string file in System.IO.Directory.GetFiles(NormalModelsDBFolder, "*" + NormalModelsFileExtension))
                            initList.Add(IO_Util.RestoreExtendedModelInfo(file).info);
                    }
                    catch
                    {
                        Console.WriteLine($"Error: unable to load model (*{NormalModelsFileExtension}) files.");
                        initList = new List<MODEL>();
                    }
                }

                // remove ALL NormalModelsFileExtension files within NormalModelsDBFolder
                if (input == "remove")
                {
                    try
                    {
                        foreach (string file in System.IO.Directory.GetFiles(NormalModelsDBFolder, "*" + NormalModelsFileExtension))
                            System.IO.File.Delete(file);
                    }
                    catch
                    {
                        Console.WriteLine($"Error: unable to delete *{NormalModelsFileExtension} files.");
                    }
                }

                // if input == "n" then nothing happened.

                Console.WriteLine();
                AnomalyDetectionController.InitADM(new AnomalyDetectorsManager(initList));
            } else {
                // no old normal-model, than start new fresh session
                AnomalyDetectionController.InitADM(new AnomalyDetectorsManager(new List<MODEL>()));
            }

            // run the server using mvc of .net asp core 5.0
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
