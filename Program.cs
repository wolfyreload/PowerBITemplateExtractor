﻿using System;
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

            if (operationType == OperationType.Export)
                exportAndProcessFiles(path, fileName);
            else
                importFromFiles(path, fileName);
        }

        private static void importFromFiles(string path, string fileName)
        {
            //make a clone of the folder we working with as we want to change the encodings of a couple of files
            var sourcePath = new DirectoryInfo(path);
            var destinationPath = new DirectoryInfo("Clone");
            if (destinationPath.Exists) destinationPath.Delete(recursive: true);
            copyFilesRecursively(sourcePath, destinationPath);

            makeFileUnicode(Path.Combine("Clone", "DataModelSchema"));
            makeFileUnicode(Path.Combine("Clone", @"Report\Layout"));
            makeFileUnicode(Path.Combine("Clone", "DiagramState"));

            //generate the zip file
            File.Delete(fileName);
            string oldCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(destinationPath.FullName);
            lauch7zip(string.Format(@"a -tZip ..\{0} * -mx9", fileName));
            Directory.SetCurrentDirectory(oldCurrentDirectory);

            //delete the clone folder as we done with it
            if (destinationPath.Exists) destinationPath.Delete(recursive: true);
     
        }

        public static void copyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (dir.Name != "DataMashupSourceData")
                    copyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        private static void exportAndProcessFiles(string path, string fileName)
        {
            if (!File.Exists(fileName)) return;
            //export to folder
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
            Directory.CreateDirectory(path);
            lauch7zip(string.Format("x {0} -o{1}", fileName, path));

            //prettyfy the json
            makeJsonPretty(Path.Combine(path, "DataModelSchema"));
            makeJsonPretty(Path.Combine(path, @"Report\Layout"));
            makeJsonPretty(Path.Combine(path, "DiagramState"));

            //extract the mashupdata
            string mashupFileLocation = Path.Combine(path, "DataMashup");
            string mashupDestinationLocation = Path.Combine(path, "DataMashupSourceData");

            lauch7zip(string.Format("x {0} -o{1}", mashupFileLocation, mashupDestinationLocation));


            //delete unneeded files
            File.Delete(Path.Combine(path, "SecurityBindings"));
            File.Delete(Path.Combine(path, @"Report\LinguisticSchema"));
        }

        private static void makeFileUnicode(string filePath)
        {
            string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
            JObject jsonObjects = JObject.Parse(jsonString);

            //collapse properties so they work in powerbi
            string[] propertiesToColapse = { "config", "query", "dataTransforms", "filters" };
            collapseJsonProperties(jsonObjects, propertiesToColapse);

            var outputEncoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
            jsonString = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
            File.WriteAllText(filePath, jsonString, outputEncoding);

        }

        private static void makeJsonPretty(string filePath)
        {
            //convert json string to jsonObjects
            string jsonString = File.ReadAllText(filePath, Encoding.Unicode);
            JObject jsonObjects = JObject.Parse(jsonString);

            //remove useless properties
            string[] propertiesToRemove = { "createdTimestamp", "lastUpdate", "lastSchemaUpdate", "lastProcessed", "modifiedTime", "structureModifiedTime", "refreshedTime" };
            removeUselessJsonProperties(jsonObjects, propertiesToRemove);

            //expand properties so we can see changes in them
            string[] propertiesToExpand = { "config", "query", "dataTransforms", "filters"  };
            expandJsonProperties(jsonObjects, propertiesToExpand);

            //sort the json files so we can check them in source control
            string[] propertiesToSortBy = { "x", "y", "z" };
            sortJsonProperties(jsonObjects, propertiesToSortBy);

            //convert back to a json string
            jsonString = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
            File.WriteAllText(filePath, jsonString, Encoding.UTF8);
        }

        private static void removeUselessJsonProperties(JToken token, string[] propertiesToRemove)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>().ToList())
                {
                    bool removed = false;

                    if (propertiesToRemove.Contains(property.Name))
                    {
                        property.Remove();
                        removed = true;
                    }

                    if (!removed)
                    {
                        removeUselessJsonProperties(property.Value, propertiesToRemove);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token.Children())
                {
                    removeUselessJsonProperties(child, propertiesToRemove);
                }
            }
        }

        private static void expandJsonProperties(JToken token, string[] propertiesToExpand)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>().ToList())
                {
                    bool processed = false;

                    if (propertiesToExpand.Contains(property.Name))
                    {
                        string jsonStringProperty = property.Value.ToString();
                        if (jsonStringProperty.StartsWith("{"))
                        {
                            JToken expandedProperties = JObject.Parse(jsonStringProperty);
                            property.Value = expandedProperties;
                            processed = true;
                        }
                        else if (jsonStringProperty.StartsWith("["))
                        {
                            JToken expandedProperties = JArray.Parse(jsonStringProperty);
                            property.Value = expandedProperties;
                            processed = true;
                        }
                    }

                    if (!processed)
                    {
                        expandJsonProperties(property.Value, propertiesToExpand);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token.Children())
                {
                    expandJsonProperties(child, propertiesToExpand);
                }
            }
        }

        private static void sortJsonProperties(JToken token, string[] propertiesToSortBy)
        {
            if (token.Type == JTokenType.Array)
            {
                var array = token as JArray;
                var firstObject = array.First as JToken;
                if (firstObject is JObject)
                {
                    bool hasX = (firstObject as JObject)["x"] != null;
                    if (hasX)
                    {
                        var newArray = new JArray(array.OrderBy(s => s["x"]).ThenBy(s => s["y"]).ThenBy(s => s["z"]));
                        array.Clear();
                        foreach (JToken item in newArray.Children())
                        {
                            array.Add(item);
                        }
                    }
                }

                foreach (JToken child in token.Children())
                {
                    sortJsonProperties(child, propertiesToSortBy);
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>().ToList())
                {
                    sortJsonProperties(property.Value, propertiesToSortBy);
                }
            }
        }

        private static void collapseJsonProperties(JToken token, string[] propertiesToCollapse)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>().ToList())
                {
                    bool processed = false;

                    if (propertiesToCollapse.Contains(property.Name))
                    {
                        string jsonStringProperty = property.Value.ToString();
                        if (property.Value.Type == JTokenType.Object || property.Value.Type == JTokenType.Array)
                        {
                            var jsonString = JsonConvert.SerializeObject(property.Value, Formatting.None);
                            property.Value = jsonString;
                            processed = true;
                        }
                    }

                    if (!processed)
                    {
                        collapseJsonProperties(property.Value, propertiesToCollapse);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token.Children())
                {
                    collapseJsonProperties(child, propertiesToCollapse);
                }
            }
        }

        /// <summary>
        /// Launch the legacy application with some options set.
        /// </summary>
        static void lauch7zip(string arguments)
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



        private static void showIncorrectUse()
        {
            Console.Write("Expecting\r\nConductor4SQL.PowerBIExtract.exe --Export --Path <Path> File.pbit\r\nor\r\nConductor4SQL.PowerBIExtract.exe --Import --Path <Path> File.pbit\r\n");
            return;
        }
    }
}
