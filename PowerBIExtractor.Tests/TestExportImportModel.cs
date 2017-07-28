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
            string configString = File.ReadAllText(".\\PowerBISourceControlConfig.json");
            var options = JsonConvert.DeserializeObject<SourceControlOptionsRoot>(configString);

            const string exportPath = @".\TestPowerBIExportSource";
            PowerBIUtil.ExportPowerBIModelToSourceFiles(exportPath, "TestPowerBIExport.pbit", options);

            var files = Directory.GetFiles(exportPath, "*", SearchOption.AllDirectories);
            Assert.IsTrue(files.Length > 0);

        }

        [TestMethod]
        public void TestImportModel()
        {
            string configString = File.ReadAllText(".\\PowerBISourceControlConfig.json");
            var options = JsonConvert.DeserializeObject<SourceControlOptionsRoot>(configString);

            const string exportPath = @".\TestPowerBIImportSource";
            PowerBIUtil.ImportPowerBIModelFromSourceFiles(exportPath, "TestPowerBIImport.pbit", options);

            bool fileExists = File.Exists("TestPowerBIImport.pbit");
            Assert.IsTrue(fileExists);
        }

    }
}
