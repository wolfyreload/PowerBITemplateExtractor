using System;
using System.Collections.Generic;
using System.Text;

namespace PowerBIExtractor
{
    public class SourceControlOption
    {
        public string FileName { get; set; }
        public string[] PropertiesToRemove { get; set; }
        public string[] propertiesToExpand { get; set; }
    }

    public class SourceControlOptionsRoot
    {
        public SourceControlOption[] SourceControlOptions { get; set; }
    }
}
