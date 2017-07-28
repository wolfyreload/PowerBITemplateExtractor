using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBIExtractor
{
    public class DaxExpression
    {
        public string MeasureTableName { get; set; }
        public string MeasureName { get; set; }
        public string MeasureExpression { get; set; }
        public bool Processed { get; set; }
    }
}
