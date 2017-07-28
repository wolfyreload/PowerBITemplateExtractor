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
                convertSourceFilesToWorkWithPowerBI(option);
            }

            //delete the dax file
            string daxMeasureFile = Path.Combine("Clone", "DaxMeasures.txt");
            if (File.Exists(daxMeasureFile))
                File.Delete(daxMeasureFile);

            //generate the powerbi file
            File.Delete(fileName);
            string oldCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(destinationPath.FullName);
            lauch7zip(string.Format(@"a -tZip ..\{0} * -mx9", fileName));
            Directory.SetCurrentDirectory(oldCurrentDirectory);

            //delete the clone folder as we done with it
            if (destinationPath.Exists) destinationPath.Delete(recursive: true);

        }

        public static void ExportPowerBIModelToSourceFiles(string destinationPath, string fileName, SourceControlOptionsRoot options)
        {
            if (!File.Exists(fileName)) return;
            //export to folder
            if (Directory.Exists(destinationPath))
                Directory.Delete(destinationPath, recursive: true);
            Directory.CreateDirectory(destinationPath);
            lauch7zip(string.Format("x {0} -o{1}", fileName, destinationPath));

            //extract the mashupdata
            string mashupFileLocation = Path.Combine(destinationPath, "DataMashup");
            string mashupDestinationPath = Path.Combine(destinationPath, "DataMashupSourceData");
            lauch7zip(string.Format("x {0} -o{1}", mashupFileLocation, mashupDestinationPath));

            //adjust the json files to work better with source control
            foreach (var option in options.SourceControlOptions)
            {
                convertSourceFilesToWorkWithGit(destinationPath, option);
            }
        }

        private static void convertSourceFilesToWorkWithGit(string destinationPath, SourceControlOption option)
        {

            //get full file path
            string filePath = Path.Combine(destinationPath, option.FileName);

            //check if unneeded file
            if (option.DeleteFile)
            {
                File.Delete(filePath);
                return;
            }

            //convert json string to jsonObjects
            string jsonString = File.ReadAllText(filePath, Encoding.Unicode);
            JObject jsonObjects = JObject.Parse(jsonString);

            //remove useless properties
            string[] propertiesToRemove = option.PropertiesToRemove;
            JsonUtil.RemoveJsonProperties(jsonObjects, propertiesToRemove);

            //expand properties so we can see changes in them
            string[] propertiesToExpand = option.PropertiesToExpand;
            JsonUtil.ExpandJsonProperties(jsonObjects, propertiesToExpand);

            //extract all the Dax information to a flat file
            if (option.ExportDaxToFile)
            {
                string daxInformation = DaxUtil.GetDaxData(jsonObjects);
                string daxStorageLocation = Path.Combine(destinationPath, "DaxMeasures.md");
                File.WriteAllText(daxStorageLocation, daxInformation);
            }

            //sort the json files so we can check them in source control
            jsonObjects = JsonUtil.SortPropertiesAlphabetically(jsonObjects);

            //convert back to a json string
            jsonString = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
            File.WriteAllText(filePath, jsonString, Encoding.UTF8);

            //rename the file with new extension
            if (option.AddFileExtension != null)
            {
                var newFilePath = filePath + option.AddFileExtension;
                File.Move(filePath, newFilePath);
            }

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

  
        private static void convertSourceFilesToWorkWithPowerBI(SourceControlOption option)
        {
            string filePath = Path.Combine("Clone", option.FileName);

            //if its a file that is deleted we dont need to convert the file
            if (option.DeleteFile)
                return;

            //rename file if we have added an extension
            if (option.AddFileExtension != null)
            {
                var filePathWithExtension = filePath + option.AddFileExtension;
                File.Move(filePathWithExtension, filePath);
            }

            //read in the file from the file system
            string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
            JObject jsonObjects = JObject.Parse(jsonString);

            //collapse properties so they work in powerbi
            string[] propertiesToColapse = option.PropertiesToExpand;
            JsonUtil.CollapseJsonProperties(jsonObjects, propertiesToColapse);

            //write back the dax data into the model
            if (option.ExportDaxToFile)
            {
                string daxStorageLocation = Path.Combine("Clone", "DaxMeasures.md");
                DaxUtil.WriteDaxData(jsonObjects, daxStorageLocation);
            }

            var outputEncoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
            jsonString = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
            File.WriteAllText(filePath, jsonString, outputEncoding);

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
