using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace PowerBIExtractor.Tests
{
    [TestClass]
    public class TestExportImportModel
    {
        [TestMethod]
        public void TestExportModel()
        {
            SourceControlOptionsRoot options = getConfig();

            const string exportPath = @".\TestPowerBIExportSource";
            PowerBIUtil.ExportPowerBIModelToSourceFiles(exportPath, "TestPowerBIExport.pbit", options);

            var files = Directory.GetFiles(exportPath, "*", SearchOption.AllDirectories);
            Assert.IsTrue(files.Length > 0);

        }

        [TestMethod]
        public void TestImportModel()
        {
            SourceControlOptionsRoot options = getConfig();

            const string importPath = @".\TestPowerBIImportSource";
            PowerBIUtil.ImportPowerBIModelFromSourceFiles(importPath, "TestPowerBIImport.pbit", options);

            bool fileExists = File.Exists("TestPowerBIImport.pbit");
            Assert.IsTrue(fileExists);
        }

        [TestMethod]
        public void TestExportFollowedByImportModel()
        {
            SourceControlOptionsRoot options = getConfig();

            const string exportPath = @".\TestPowerBIExportFollowedByImportSource";
            PowerBIUtil.ExportPowerBIModelToSourceFiles(exportPath, "TestPowerBIImport.pbit", options);
            PowerBIUtil.ImportPowerBIModelFromSourceFiles(exportPath, "TestPowerBIExportFollowedByImport.pbit", options);


            bool fileExists = File.Exists("TestPowerBIExportFollowedByImport.pbit");
            Assert.IsTrue(fileExists);
        }

        private static SourceControlOptionsRoot getConfig()
        {
            string configString = File.ReadAllText(".\\PowerBISourceControlConfig.json");
            var options = JsonConvert.DeserializeObject<SourceControlOptionsRoot>(configString);
            return options;
        }
    }
}
