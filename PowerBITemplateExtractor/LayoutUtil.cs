using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PowerBITemplateExtractor
{
    public class LayoutUtil
    {
        internal static void ExtractLayouts(JObject jsonObjects, string layoutStorageLocation)
        {
            //make directory for the layout data if it doesn't exist
            if (!Directory.Exists(layoutStorageLocation))
                Directory.CreateDirectory(layoutStorageLocation);

            //extract every section and write each report to a new file
            var sections = jsonObjects["sections"];
            foreach (var section in sections.Children())
            {
                string nameOfSection = section["displayName"].ToString();
                string layoutFileLocation = Path.Combine(layoutStorageLocation, nameOfSection) + ".json";
                File.WriteAllText(layoutFileLocation, section.ToString());
            }
            jsonObjects["sections"] = new JArray();

            //extract the bookmarks section
            string bookmarkFileLocation = Path.Combine(layoutStorageLocation, "bookmarks") + ".json";
            var bookmarks = jsonObjects["config"]["bookmarks"];
            if (bookmarks != null)
            {
                File.WriteAllText(bookmarkFileLocation, bookmarks.ToString());
                jsonObjects["config"]["bookmarks"] = new JArray();
            }

        }


        public static void WriteLayouts(JToken jsonObjects, string layoutStorageLocation)
        {
            //make directory for the layout data if it doesn't exist
            if (!Directory.Exists(layoutStorageLocation))
                Directory.CreateDirectory(layoutStorageLocation);

            var layoutSectionFileNames = Directory.GetFiles(layoutStorageLocation, "*");

            //if we don't have any layout section files we using an older version of the PowerBI reports and we have nothing to do
            if (layoutSectionFileNames.Count() == 0)
                return;

            Dictionary<string, string> layoutFileContent = new Dictionary<string, string>();
            foreach (string layoutSectionFileName in layoutSectionFileNames)
            {
                string fileContent = File.ReadAllText(layoutSectionFileName);
                string shortPath = new FileInfo(layoutSectionFileName).Name;
                layoutFileContent[shortPath] = fileContent;
            }
            Directory.Delete(layoutStorageLocation, true);

            //remove the bookmarks file from the list and put it back in the layout area
            if (layoutFileContent.Keys.Contains("bookmarks.json"))
            {
                JArray bookmarksExternal = JArray.Parse(layoutFileContent["bookmarks.json"]);
                jsonObjects["config"]["bookmarks"] = bookmarksExternal;
                layoutFileContent.Remove("bookmarks.json");
            }

            //get all the sections as JTokens that we can add to the the sections tag and order them by the ordinal property
            List<JToken> sectionsList = new List<JToken>();
            foreach (string key in layoutFileContent.Keys)
            {
                sectionsList.Add(JObject.Parse(layoutFileContent[key]));
            }
            sectionsList = sectionsList.OrderBy(s => s["ordinal"].ToString()).ToList();

            //add all the sections back into the file
            JArray sections = jsonObjects["sections"] as JArray;
            foreach (var section in sectionsList)
            {
                sections.Add(section);
            }
        }

    }
}
