using System;
using System.Collections.Generic;
using System.Text;

namespace PowerBITemplateExtractor
{
    public class SourceControlOption
    {
        public string FileName { get; set; }
        public List<string> PropertiesToRemove { get; set; }
        public List<string> PropertiesToExpand { get; set; }
        public bool ExportDaxToFile { get; set; }
        public bool DeleteFile { get; set; }
        public string AddFileExtension { get; set; }

        public SourceControlOption()
        {
            AddFileExtension = "";
            PropertiesToExpand = new List<string>();
            PropertiesToRemove = new List<string>();
        }
    }

    public class SourceControlOptionsRoot
    {
        public SourceControlOption[] SourceControlOptions { get; set; }
        public string PowerBITemplatePath { get; set; }
        public string PowerBISourceControlPath { get; set; }
    }
}
