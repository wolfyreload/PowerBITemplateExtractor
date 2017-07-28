using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBIExtractor
{
    public class DaxUtil
    {
        public static string GetDaxData(JToken jsonObjects)
        {
            StringBuilder builder = new StringBuilder();
            List<JToken> measureTables = jsonObjects.SelectTokens("$..measures").ToList();
            measureTables = measureTables.OrderBy(m => m.Parent.Parent["name"].ToString().ToLower()).ToList();
            foreach (JToken measureTable in measureTables)
            {
                List<JToken> measures = measureTable.Children().OrderBy(m => m["name"].ToString().ToLower()).ToList();
                string measureTableName = measureTable.Parent.Parent["name"].ToString();
                builder.AppendLine(measureTableName);
                builder.AppendLine("=============================");
                builder.AppendLine();

                foreach (JToken measure in measures)
                {
                    string measureName = measure["name"].ToString();
                    string measureExpression = convertToWindowsNewLines(measure["expression"].ToString()).Trim();

                    //blank out the expression as we going to put it in another file
                    measure["expression"] = "";

                    //build the dax expression
                    builder.AppendLine("```DAX");
                    builder.Append(measureName + " =");
                    builder.AppendLine(measureExpression);
                    builder.AppendLine("```");
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        public static void WriteDaxData(JToken jsonObjects, string pathToDaxFile)
        {
            //get the dax expressions from the dax md file
            List<DaxExpression> DaxExpressions = getDaxExpressionsFromFile(pathToDaxFile);

            List<JToken> measureTables = jsonObjects.SelectTokens("$..measures").ToList();
            measureTables = measureTables.OrderBy(m => m.Parent.Parent["name"].ToString().ToLower()).ToList();
            foreach (var measureTable in measureTables)
            {
                //update existing measures
                List<JToken> listOfMeasuresToRemove = new List<JToken>();
                string measureTableName = measureTable.Parent.Parent["name"].ToString();
                List<JToken> measuresInPowerBI = measureTable.Children().OrderBy(m => m["name"].ToString().ToLower()).ToList();
                var measuresInDaxFile = DaxExpressions.Where(d => d.MeasureTableName == measureTableName).ToList();
                foreach (JToken measure in measuresInPowerBI)
                {
                    string measureName = measure["name"].ToString();
                    var daxFileMeasure = measuresInDaxFile.FirstOrDefault(m => m.MeasureName == measureName);
                    if (daxFileMeasure != null)
                    {
                        measure["expression"] = daxFileMeasure.MeasureExpression;
                        daxFileMeasure.Processed = true;
                    }
                    else
                    {
                        listOfMeasuresToRemove.Add(measure);
                    }
                }

                //add new measures that are in the dax file that are not in the model
                foreach (var measureToAdd in measuresInDaxFile.Where(m => m.Processed == false))
                {
                    var jsonObjectToAdd = JObject.FromObject(new
                    {
                        name = measureToAdd.MeasureName,
                        expression = measureToAdd.MeasureExpression
                    });

                    ((JArray)measureTable).Add(jsonObjectToAdd);
                }

                //remove measures that are not in the dax file
                foreach (var measureToRemove in listOfMeasuresToRemove)
                {
                    measureToRemove.Remove();
                }

             }

             
        }

        private static List<DaxExpression> getDaxExpressionsFromFile(string pathToDaxFile)
        {
            List<DaxExpression> DaxExpressions = new List<DaxExpression>();
            string measureTableName = null;
            string[] fileLines = File.ReadAllLines(pathToDaxFile);
            bool isDaxLine = false;
            string measureName = null;
            StringBuilder measureBuilder = new StringBuilder();

            for (int i=0; i < fileLines.Length; i++)
            {
                string fileLine = fileLines[i];

                if (fileLine.Trim() == "=============================")
                    measureTableName = fileLines[i - 1];

                //cleanup if isn't a dax line
                if (!isDaxLine)
                {
                    measureName = "";
                    measureBuilder.Length = 0;
                }

                //check if we at the start or end of a piece of dax
                if (fileLine.Trim() == "```DAX")
                    isDaxLine = true;
                else if (fileLine.Trim() == "```")
                    isDaxLine = false;

                if (isDaxLine)
                {
                    if (String.IsNullOrWhiteSpace(measureName) && fileLine.Contains("="))
                    {
                        int indexOfEqualsSign = fileLine.IndexOf("=");
                        measureName = fileLine.Substring(0, indexOfEqualsSign);
                        measureBuilder.AppendLine(fileLine.Substring(indexOfEqualsSign + 1));
                    }
                    else if (!String.IsNullOrWhiteSpace(measureName))
                    {
                        measureBuilder.AppendLine(fileLine);
                    }
                }

                if (isDaxLine == false && !String.IsNullOrWhiteSpace(measureName))
                {
                    DaxExpressions.Add(new DaxExpression()
                    {
                        MeasureTableName = measureTableName.Trim(),
                        MeasureName = measureName.Trim(),
                        MeasureExpression = measureBuilder.ToString()
                    });
                }
            }

            return DaxExpressions;
        }

        private static string convertToWindowsNewLines(string text)
        {
            return text.Replace("\r\n", "\r").Replace("\n", "\r").Replace("\r", "\r\n");
        }

        private static string convertToPowerBINewLines(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}
