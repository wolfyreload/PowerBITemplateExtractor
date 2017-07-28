using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PowerBIExtractor
{
    enum OperationType { Export, Import }

    class Program
    {
        static void Main(string[] args)
        {
            OperationType operationType;
            string path = null;
            string fileName = null;

            if (args.Count() != 4)
            {
                showIncorrectUse();
                return;
            }

            if (args[0] != "--Export" && args[0] != "--Import")
            {
                showIncorrectUse();
                return;
            }

            if (args[1] != "--Path")
            {
                showIncorrectUse();
                return;
            }

            operationType = args[0] == "--Export" ? OperationType.Export : OperationType.Import;
            path = args[2];
            fileName = args[3];

            string configString = File.ReadAllText(".\\PowerBISourceControlConfig.json");
            var options = JsonConvert.DeserializeObject<SourceControlOptionsRoot>(configString);

            if (operationType == OperationType.Export)
                PowerBIUtil.ExportPowerBIModelToSourceFiles(path, fileName, options);
            else
                PowerBIUtil.ImportPowerBIModelFromSourceFiles(path, fileName, options);
        }

   



        private static void showIncorrectUse()
        {
            Console.Write("Expecting\r\nConductor4SQL.PowerBIExtract.exe --Export --Path <Path> File.pbit\r\nor\r\nConductor4SQL.PowerBIExtract.exe --Import --Path <Path> File.pbit\r\n");
            return;
        }
    }
}
