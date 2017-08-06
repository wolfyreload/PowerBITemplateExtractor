using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PowerBITemplateExtractor
{
    enum OperationType { Export, Import }

    class Program
    {
        static void Main(string[] args)
        {
            OperationType operationType;
            string configPath = null;

            if (args.Count() != 3)
            {
                showIncorrectUse();
                return;
            }

            if (args[0] != "--Export" && args[0] != "--Import")
            {
                showIncorrectUse();
                return;
            }

            if (args[1] != "--Config")
            {
                showIncorrectUse();
                return;
            }

            operationType = args[0] == "--Export" ? OperationType.Export : OperationType.Import;
            configPath = args[2];

            if (!File.Exists(configPath))
            {
                Console.WriteLine(string.Format("Config file '{configPath}' does not exist", configPath));
                return;
            }

            string configString = File.ReadAllText(configPath);
            var options = JsonConvert.DeserializeObject<SourceControlOptionsRoot>(configString);

            if (operationType == OperationType.Export)
                PowerBIUtil.ExportPowerBIModelToSourceFiles(options);
            else
                PowerBIUtil.ImportPowerBIModelFromSourceFiles(options);
        }

   



        private static void showIncorrectUse()
        {
            Console.WriteLine(@"
PowerBI Template Extractor
Usage: PowerbITemplateExtractor [command] [arguments]

Arguments:
  [command]             The command to execute
  [arguments]           Arguments to pass to the command

Common options:
  -h|--help             Show help

Commands:
  Export        Exports a PowerBI template file into source files
  Import        Imports a PowerBI template file from source files

Arguments:
  --Config      The path to the configuration file 
");            
        }
    }
}
