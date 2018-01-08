using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBITemplateExtractor
{
    public class PowerBIUtil
    {
        public static void ImportPowerBIModelFromSourceFiles(SourceControlOptionsRoot options)
        {
            string fileName = options.PowerBITemplatePath;
            string sourcePath = options.PowerBISourceControlPath;

            //make a clone of the folder we working with as we want to change the encodings of a couple of files
            string clonePath = new DirectoryInfo(sourcePath).Parent.FullName + "\\clone";
            deleteDirectory(clonePath);
            copyFilesRecursively(sourcePath, clonePath);

            foreach (var option in options.SourceControlOptions)
            {
                convertSourceFilesToWorkWithPowerBI(clonePath, option);
            }

            //generate the powerbi file
            File.Delete(fileName);
            ZipUtil.CreateArchive(clonePath, fileName);

            //delete the clone folder as we done with it
            deleteDirectory(clonePath);
        }

        private static void deleteDirectory(string clonePath)
        {
            var directoryInfo = new DirectoryInfo(clonePath);
            directoryInfo.Refresh();
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(recursive: true);
            }
        }

        public static void ExportPowerBIModelToSourceFiles(SourceControlOptionsRoot options)
        {
            string fileName = options.PowerBITemplatePath;
            string destinationPath = options.PowerBISourceControlPath;

            if (!File.Exists(fileName)) return;
            //export to folder
            ZipUtil.ExtractArchive(destinationPath, fileName);

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

            //if the file we working with is an archive extract that file
            if (option.TreatAsArchive)
            {
                string archivePath = Path.Combine(destinationPath, option.FileName);
                string archiveExtractDestinationPath = Path.Combine(destinationPath, option.ArchiveDestinationPath);
                ZipUtil.ExtractArchive(archiveExtractDestinationPath, archivePath);
            }

            if (option.IsJsonFile)
            {
                //convert json string to jsonObjects
                string jsonString = File.ReadAllText(filePath, Encoding.Unicode);
                JObject jsonObjects = JObject.Parse(jsonString);

                //remove useless properties
                var propertiesToRemove = option.PropertiesToRemove;
                JsonUtil.RemoveJsonProperties(jsonObjects, propertiesToRemove);

                //expand properties so we can see changes in them
                var propertiesToExpand = option.PropertiesToExpand;
                JsonUtil.ExpandJsonProperties(jsonObjects, propertiesToExpand);

                //extract all the Dax information to a flat file
                if (option.ExportDaxToFile)
                {
                    string daxInformation = DaxUtil.GetDaxData(jsonObjects);
                    string daxStorageLocation = Path.Combine(destinationPath, "DaxMeasures.md");
                    File.WriteAllText(daxStorageLocation, daxInformation);
                }

                //extract layout file
                if (option.FileName == "Report\\Layout")
                {
                    string layoutStorageLocation = Path.Combine(destinationPath, "Report", "LayoutFiles");
                    LayoutUtil.ExtractLayouts(jsonObjects, layoutStorageLocation);
                }

                //sort the json files so we can check them in source control
                jsonObjects = JsonUtil.SortPropertiesAlphabetically(jsonObjects);

                //convert back to a json string
                jsonString = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
                File.WriteAllText(filePath, jsonString, Encoding.UTF8);
            }

            //rename the file with new extension
            if (!string.IsNullOrWhiteSpace(option.AddFileExtension))
            {
                var newFilePath = filePath + option.AddFileExtension;
                renameFile(filePath, newFilePath);
            }

        }

        private static void copyFilesRecursively(string sourcePath, string targetPath)
        {
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            DirectoryInfo targetDirectoryInfo = new DirectoryInfo(targetPath);

            foreach (DirectoryInfo subDirectory in sourceDirectoryInfo.GetDirectories())
            {
                var newTargetSubDirectory = targetDirectoryInfo.CreateSubdirectory(subDirectory.Name);
                copyFilesRecursively(subDirectory.FullName, newTargetSubDirectory.FullName);
            }
            foreach (FileInfo file in sourceDirectoryInfo.GetFiles())
                file.CopyTo(Path.Combine(targetDirectoryInfo.FullName, file.Name));
        }


        private static void convertSourceFilesToWorkWithPowerBI(string sourcePath, SourceControlOption option)
        {
            string filePath = Path.Combine(sourcePath, option.FileName);

            //if its a file that is deleted we dont need to convert the file
            if (option.DeleteFile)
                return;

            if (option.IsJsonFile)
            {
                //rename file if we have added an extension
                if (!string.IsNullOrWhiteSpace(option.AddFileExtension))
                {
                    var filePathWithExtension = filePath + option.AddFileExtension;
                    renameFile(filePathWithExtension, filePath);
                }

                //read in the file from the file system
                string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
                JObject jsonObjects = JObject.Parse(jsonString);

                //restore layout file
                if (option.FileName == "Report\\Layout")
                {
                    string layoutStorageLocation = Path.Combine(sourcePath, "Report", "LayoutFiles");
                    LayoutUtil.WriteLayouts(jsonObjects, layoutStorageLocation);
                }

                //collapse properties so they work in powerbi
                var propertiesToColapse = option.PropertiesToExpand;
                JsonUtil.CollapseJsonProperties(jsonObjects, propertiesToColapse);

                //write back the dax data into the model
                if (option.ExportDaxToFile)
                {
                    string daxStorageLocation = Path.Combine(sourcePath, "DaxMeasures.md");
                    DaxUtil.WriteDaxData(jsonObjects, daxStorageLocation);
                }

                var outputEncoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
                jsonString = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
                File.WriteAllText(filePath, jsonString, outputEncoding);
            }
        }

        private static void renameFile(string oldFulePath, string newFilePath)
        {
            File.Delete(newFilePath);
            File.Copy(oldFulePath, newFilePath);
            File.Delete(oldFulePath);
        }

    }
}
