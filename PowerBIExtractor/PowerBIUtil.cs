using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBIExtractor
{
    public class PowerBIUtil
    {
        public static void ImportPowerBIModelFromSourceFiles(string path, string fileName, SourceControlOptionsRoot options)
        {
            //make a clone of the folder we working with as we want to change the encodings of a couple of files
            var sourcePath = new DirectoryInfo(path);
            var destinationPath = new DirectoryInfo("Clone");
            if (destinationPath.Exists) destinationPath.Delete(recursive: true);
            copyFilesRecursively(sourcePath, destinationPath);

            foreach (var option in options.SourceControlOptions)
            {
                makeFileUnicode(option);
            }

            //generate the zip file
            File.Delete(fileName);
            string oldCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(destinationPath.FullName);
            lauch7zip(string.Format(@"a -tZip ..\{0} * -mx9", fileName));
            Directory.SetCurrentDirectory(oldCurrentDirectory);

            //delete the clone folder as we done with it
            if (destinationPath.Exists) destinationPath.Delete(recursive: true);

        }

        public static void ExportPowerBIModelToSourceFiles(string path, string fileName, SourceControlOptionsRoot options)
        {
            if (!File.Exists(fileName)) return;
            //export to folder
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
            Directory.CreateDirectory(path);
            lauch7zip(string.Format("x {0} -o{1}", fileName, path));

            //extract the mashupdata
            string mashupFileLocation = Path.Combine(path, "DataMashup");
            string mashupDestinationLocation = Path.Combine(path, "DataMashupSourceData");
            lauch7zip(string.Format("x {0} -o{1}", mashupFileLocation, mashupDestinationLocation));

            //prettyfy the json
            foreach (var option in options.SourceControlOptions)
            {
                var jsonObjects = makeJsonPretty(path, option);

                //extract all the Dax information
                if (option.ExportDaxToFile)
                {
                    string daxInformation = JsonUtil.GetDaxData(jsonObjects);
                    string daxStorageLocation = Path.Combine(mashupDestinationLocation, "Formulas", "DaxMeasures.txt");
                    File.WriteAllText(daxStorageLocation, daxInformation);

                }
            }

            //delete unneeded files
            File.Delete(Path.Combine(path, "SecurityBindings"));
            File.Delete(Path.Combine(path, @"Report\LinguisticSchema"));
        }


        private static void copyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (dir.Name != "DataMashupSourceData")
                    copyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

  
        private static void makeFileUnicode(SourceControlOption option)
        {
            string filePath = Path.Combine("Clone", option.FileName);

            string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
            JObject jsonObjects = JObject.Parse(jsonString);

            //collapse properties so they work in powerbi
            string[] propertiesToColapse = option.propertiesToExpand;
            JsonUtil.CollapseJsonProperties(jsonObjects, propertiesToColapse);

            var outputEncoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
            jsonString = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
            File.WriteAllText(filePath, jsonString, outputEncoding);

        }

        private static JToken makeJsonPretty(string basePath, SourceControlOption option)
        {
            string filePath = Path.Combine(basePath, option.FileName);

            //convert json string to jsonObjects
            string jsonString = File.ReadAllText(filePath, Encoding.Unicode);
            JObject jsonObjects = JObject.Parse(jsonString);

            //remove useless properties
            string[] propertiesToRemove = option.PropertiesToRemove;
            JsonUtil.RemoveJsonProperties(jsonObjects, propertiesToRemove);

            //expand properties so we can see changes in them
            string[] propertiesToExpand = option.propertiesToExpand;
            JsonUtil.ExpandJsonProperties(jsonObjects, propertiesToExpand);

            //sort the json files so we can check them in source control
            jsonObjects = JsonUtil.SortPropertiesAlphabetically(jsonObjects);

            //convert back to a json string
            jsonString = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
            File.WriteAllText(filePath, jsonString, Encoding.UTF8);

            return jsonObjects;
        }



        /// <summary>
        /// Launch the legacy application with some options set.
        /// </summary>
        private static void lauch7zip(string arguments)
        {
            // For the example

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "7za.exe";
            startInfo.Arguments = arguments;
            Console.WriteLine("7za.exe " + arguments);

            // Start the process with the info we specified.
            // Call WaitForExit and then the using statement will close.
            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }
    }
}
