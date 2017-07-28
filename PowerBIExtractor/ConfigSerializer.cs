using System;
using System.Collections.Generic;
using System.Text;

namespace PowerBIExtractor
{
    public class SourceControlOption
    {
        public string FileName { get; set; }
        public string[] PropertiesToRemove { get; set; }
        public string[] PropertiesToExpand { get; set; }
        public bool ExportDaxToFile { get; set; }
        public bool DeleteFile { get; set; }
    }

    public class SourceControlOptionsRoot
    {
        public SourceControlOption[] SourceControlOptions { get; set; }
    }
}
