using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace PowerBITemplateExtractor.Tests
{
    [TestClass]
    public class TestExportImportModel
    {
        [TestMethod]
        public void TestExportModel()
        {
            SourceControlOptionsRoot options = getConfig();
            options.PowerBITemplatePath = "TestPowerBIExport.pbit";
            options.PowerBISourceControlPath = @".\TestPowerBIExportSource";
            PowerBIUtil.ExportPowerBIModelToSourceFiles(options);

            var files = Directory.GetFiles(options.PowerBISourceControlPath, "*", SearchOption.AllDirectories);
            Assert.IsTrue(files.Length > 0);

        }

        [TestMethod]
        public void TestImportModel()
        {
            SourceControlOptionsRoot options = getConfig();
            options.PowerBITemplatePath = "TestPowerBIImport.pbit";
            options.PowerBISourceControlPath = @".\TestPowerBIImportSource";
            PowerBIUtil.ImportPowerBIModelFromSourceFiles(options);

            bool fileExists = File.Exists(options.PowerBITemplatePath);
            Assert.IsTrue(fileExists);
        }

        [TestMethod]
        public void TestExportFollowedByImportModel()
        {
            SourceControlOptionsRoot options1 = getConfig();
            options1.PowerBITemplatePath = "TestPowerBIExport.pbit";
            options1.PowerBISourceControlPath = @".\TestPowerBIExportFollowedByImportSource";

            SourceControlOptionsRoot options2 = getConfig();
            options2.PowerBITemplatePath = "TestPowerBIExportFollowedByImport.pbit";
            options2.PowerBISourceControlPath = @".\TestPowerBIExportFollowedByImportSource";


            PowerBIUtil.ExportPowerBIModelToSourceFiles(options1);
            PowerBIUtil.ImportPowerBIModelFromSourceFiles(options2);


            bool fileExists = File.Exists(options1.PowerBITemplatePath);
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
